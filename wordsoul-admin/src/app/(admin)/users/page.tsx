'use client';

import React, { useState, useEffect, useCallback } from 'react';
import {
  Table, Input, Select, Button, Modal,
  Form, App, Popconfirm, Space, Avatar, Tag, Tooltip,
} from 'antd';
import {
  SearchOutlined, EditOutlined, DeleteOutlined,
  StopOutlined, CheckCircleOutlined, ExportOutlined,
} from '@ant-design/icons';
import type { TableRowSelection } from 'antd/es/table/interface';
import { authApi, endpoints } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';
import dayjs from 'dayjs';
import UserDetailDrawer from './_components/UserDetailDrawer';

interface User {
  id: number;
  username: string;
  email: string;
  role: string;
  createdAt: string;
  isActive: boolean;
}

const ROLE_STYLE: Record<string, { color: string; bg: string; border: string }> = {
  SuperAdmin: { color: 'var(--accent)',     bg: 'var(--accent-subtle)',  border: 'var(--accent-border)' },
  Admin:      { color: '#0369A1',           bg: '#F0F9FF',               border: '#BAE6FD' },
  User:       { color: 'var(--text-muted)', bg: 'var(--bg-muted)',       border: 'var(--border)' },
};

function RoleBadge({ role }: { role: string }) {
  const s = ROLE_STYLE[role] ?? ROLE_STYLE['User'];
  return (
    <span
      style={{
        fontSize: 11, fontWeight: 600, padding: '2px 7px',
        borderRadius: 4, color: s.color, background: s.bg,
        border: `1px solid ${s.border}`, whiteSpace: 'nowrap',
      }}
    >
      {role || 'User'}
    </span>
  );
}

function StatusBadge({ isActive }: { isActive: boolean }) {
  return (
    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 5 }}>
      <span style={{
        display: 'inline-block', width: 6, height: 6, borderRadius: '50%',
        background: isActive ? 'var(--success)' : 'var(--danger)',
      }} />
      <span style={{ fontSize: 12, color: isActive ? 'var(--success)' : 'var(--danger)', fontWeight: 500 }}>
        {isActive ? 'Active' : 'Banned'}
      </span>
    </span>
  );
}

// â”€â”€ CSV export helper â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
function exportToCsv(users: User[]) {
  const header = ['ID', 'Username', 'Email', 'Role', 'Status', 'Joined'];
  const rows = users.map(u => [
    u.id,
    `"${u.username}"`,
    `"${u.email}"`,
    u.role,
    u.isActive ? 'Active' : 'Banned',
    dayjs(u.createdAt).format('YYYY-MM-DD'),
  ]);
  const csv = [header, ...rows].map(r => r.join(',')).join('\n');
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `users-${dayjs().format('YYYYMMDD')}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}

export default function UsersPage() {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [roleFilter, setRoleFilter] = useState<string | null>(null);
  const { message } = App.useApp();

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(15);
  const [hasMore, setHasMore] = useState(true);

  // Role modal
  const [isRoleModalVisible, setIsRoleModalVisible] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [roleForm] = Form.useForm();

  // Bulk selection
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);

  // Detail drawer
  const [drawerUserId, setDrawerUserId] = useState<number | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const isSuperAdmin = currentUser?.role === 'SuperAdmin';

  const fetchUsers = useCallback(async (page: number, size: number, name?: string, role?: string | null) => {
    setLoading(true);
    try {
      let url = `${endpoints.users}?pageNumber=${page}&pageSize=${size}`;
      if (name) url += `&name=${encodeURIComponent(name)}`;
      if (role && role !== 'All') url += `&role=${encodeURIComponent(role)}`;
      const response = await authApi.get(url);
      setUsers(response.data);
      setHasMore(response.data.length === size);
    } catch {
      message.error('Failed to fetch users');
    } finally {
      setLoading(false);
    }
  }, [message]);

  useEffect(() => {
    fetchUsers(currentPage, pageSize, searchText, roleFilter);
  }, [currentPage, pageSize, searchText, roleFilter, fetchUsers]);

  const handleSearch = (value: string) => { setSearchText(value); setCurrentPage(1); };
  const handleRoleFilter = (value: string) => { setRoleFilter(value); setCurrentPage(1); };

  // â”€â”€ Ban / Unban â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  const handleToggleBan = async (u: User) => {
    const newStatus = !u.isActive;
    try {
      await authApi.put(endpoints.userStatus(u.id), { isActive: newStatus });
      message.success(newStatus ? `${u.username} has been unbanned` : `${u.username} has been banned`);
      fetchUsers(currentPage, pageSize, searchText, roleFilter);
    } catch {
      message.error('Failed to update user status');
    }
  };

  // â”€â”€ Delete â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  const handleDeleteUser = async (userId: number) => {
    try {
      await authApi.delete(`${endpoints.users}/${userId}`);
      message.success('User deleted');
      setSelectedRowKeys(prev => prev.filter(k => k !== userId));
      fetchUsers(currentPage, pageSize, searchText, roleFilter);
    } catch {
      message.error('Failed to delete user');
    }
  };

  // â”€â”€ Role modal â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  const openRoleModal = (u: User) => {
    setEditingUser(u);
    roleForm.setFieldsValue({ role: u.role || 'User' });
    setIsRoleModalVisible(true);
  };

  const handleUpdateRole = async (values: { role: string }) => {
    if (!editingUser) return;
    try {
      await authApi.put(`${endpoints.users}/${editingUser.id}/role`, { roleName: values.role });
      message.success('Role updated');
      setIsRoleModalVisible(false);
      fetchUsers(currentPage, pageSize, searchText, roleFilter);
    } catch {
      message.error('Failed to update role');
    }
  };

  // â”€â”€ Bulk actions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  const handleBulkBan = async (isActive: boolean) => {
    const targets = users.filter(u => selectedRowKeys.includes(u.id) && u.role !== 'SuperAdmin');
    if (targets.length === 0) { message.warning('No eligible users selected'); return; }
    let ok = 0;
    for (const u of targets) {
      try {
        await authApi.put(endpoints.userStatus(u.id), { isActive });
        ok++;
      } catch { /* continue */ }
    }
    message.success(`${ok} user(s) ${isActive ? 'unbanned' : 'banned'}`);
    setSelectedRowKeys([]);
    fetchUsers(currentPage, pageSize, searchText, roleFilter);
  };

  const handleBulkDelete = async () => {
    const targets = users.filter(u => selectedRowKeys.includes(u.id) && u.role !== 'SuperAdmin');
    let ok = 0;
    for (const u of targets) {
      try {
        await authApi.delete(`${endpoints.users}/${u.id}`);
        ok++;
      } catch { /* continue */ }
    }
    message.success(`${ok} user(s) deleted`);
    setSelectedRowKeys([]);
    fetchUsers(currentPage, pageSize, searchText, roleFilter);
  };

  // â”€â”€ Row selection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  const rowSelection: TableRowSelection<User> = {
    selectedRowKeys,
    onChange: setSelectedRowKeys,
    getCheckboxProps: (record) => ({
      disabled: record.id.toString() === currentUser?.id,
    }),
  };

  const columns = [
    {
      title: 'User',
      key: 'user',
      render: (_: unknown, record: User) => (
        <div
          style={{ display: 'flex', alignItems: 'center', gap: 10, cursor: 'pointer' }}
          onClick={() => { setDrawerUserId(record.id); setDrawerOpen(true); }}
        >
          <Avatar
            size={30}
            style={{ background: 'var(--accent-subtle)', color: 'var(--accent)', fontSize: 11, fontWeight: 600, flexShrink: 0 }}
          >
            {record.username.charAt(0).toUpperCase()}
          </Avatar>
          <div>
            <div style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-primary)' }}>
              {record.username}
            </div>
            <div style={{ fontSize: 11, color: 'var(--text-muted)' }}>{record.email}</div>
          </div>
        </div>
      ),
    },
    {
      title: 'Role',
      dataIndex: 'role',
      key: 'role',
      width: 110,
      render: (role: string) => <RoleBadge role={role} />,
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 90,
      render: (v: boolean) => <StatusBadge isActive={v} />,
    },
    {
      title: 'Joined',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 120,
      render: (d: string) => (
        <span style={{ fontSize: 12, color: 'var(--text-muted)', fontVariantNumeric: 'tabular-nums' }}>
          {dayjs(d).format('YYYY-MM-DD')}
        </span>
      ),
    },
    {
      title: '',
      key: 'action',
      width: 130,
      render: (_: unknown, record: User) => {
        const isSelf = record.id.toString() === currentUser?.id;
        const isSuperAdminTarget = record.role === 'SuperAdmin';
        const canBan = !isSelf && !isSuperAdminTarget;
        const canDelete = isSuperAdmin && !isSelf;

        return (
          <Space size={0}>
            {/* Edit role */}
            <Tooltip title="Change role">
              <Button
                type="text" size="small" icon={<EditOutlined />}
                onClick={() => openRoleModal(record)}
                disabled={!isSuperAdmin || isSelf}
                style={{ color: 'var(--text-muted)', fontSize: 13 }}
              />
            </Tooltip>

            {/* Ban / Unban */}
            <Tooltip title={record.isActive ? 'Ban user' : 'Unban user'}>
              <Popconfirm
                title={record.isActive ? `Ban ${record.username}?` : `Unban ${record.username}?`}
                description={record.isActive ? 'User will be locked out immediately.' : 'User will regain access.'}
                onConfirm={() => handleToggleBan(record)}
                okText={record.isActive ? 'Ban' : 'Unban'}
                okButtonProps={{ danger: record.isActive }}
                cancelText="Cancel"
                disabled={!canBan}
              >
                <Button
                  type="text" size="small"
                  icon={record.isActive ? <StopOutlined /> : <CheckCircleOutlined />}
                  disabled={!canBan}
                  style={{ color: record.isActive ? 'var(--danger)' : 'var(--success)', fontSize: 13 }}
                />
              </Popconfirm>
            </Tooltip>

            {/* Delete */}
            <Tooltip title="Delete user">
              <Popconfirm
                title="Delete this user?"
                description="This action cannot be undone."
                onConfirm={() => handleDeleteUser(record.id)}
                okText="Delete" okButtonProps={{ danger: true }}
                cancelText="Cancel"
                disabled={!canDelete}
              >
                <Button
                  type="text" size="small" danger
                  icon={<DeleteOutlined />}
                  disabled={!canDelete}
                />
              </Popconfirm>
            </Tooltip>
          </Space>
        );
      },
    },
  ];

  const hasBulkSelection = selectedRowKeys.length > 0;

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">User Management</h1>
          <p className="page-subtitle">View, filter, and manage platform user accounts and roles.</p>
        </div>
        <Button
          size="small"
          icon={<ExportOutlined />}
          onClick={() => exportToCsv(users)}
        >
          Export CSV
        </Button>
      </div>

      {/* Main Card */}
      <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
        {/* Filter Bar */}
        <div
          style={{
            padding: '12px 16px',
            borderBottom: '1px solid var(--border)',
            display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap',
          }}
        >
          <Input.Search
            placeholder="Search by username or emailâ€¦"
            allowClear
            onSearch={handleSearch}
            style={{ maxWidth: 280 }}
            size="small"
            prefix={<SearchOutlined style={{ color: 'var(--text-muted)' }} />}
          />
          <Select
            placeholder="All Roles"
            style={{ width: 130 }}
            allowClear
            size="small"
            onChange={handleRoleFilter}
            options={[
              { value: 'All', label: 'All Roles' },
              { value: 'User', label: 'User' },
              { value: 'Admin', label: 'Admin' },
              { value: 'SuperAdmin', label: 'SuperAdmin' },
            ]}
          />

          {/* Bulk action bar */}
          {hasBulkSelection && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginLeft: 'auto' }}>
              <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>
                {selectedRowKeys.length} selected
              </span>
              <Button size="small" onClick={() => handleBulkBan(false)} danger>
                Bulk Ban
              </Button>
              <Button size="small" onClick={() => handleBulkBan(true)} style={{ color: 'var(--success)', borderColor: 'var(--success)' }}>
                Bulk Unban
              </Button>
              {isSuperAdmin && (
                <Popconfirm
                  title={`Delete ${selectedRowKeys.length} users?`}
                  description="This cannot be undone."
                  onConfirm={handleBulkDelete}
                  okText="Delete all" okButtonProps={{ danger: true }}
                  cancelText="Cancel"
                >
                  <Button size="small" danger>
                    Bulk Delete
                  </Button>
                </Popconfirm>
              )}
              <Button size="small" onClick={() => setSelectedRowKeys([])}>
                Clear
              </Button>
            </div>
          )}
        </div>

        {/* Table */}
        <Table
          rowSelection={rowSelection}
          columns={columns}
          dataSource={users}
          rowKey="id"
          loading={loading}
          size="small"
          style={{ fontSize: 13 }}
          onRow={(record) => ({
            style: {
              opacity: record.isActive ? 1 : 0.55,
              background: selectedRowKeys.includes(record.id) ? 'var(--accent-subtle)' : undefined,
            },
          })}
          pagination={{
            current: currentPage,
            pageSize,
            total: hasMore ? currentPage * pageSize + 1 : currentPage * pageSize,
            onChange: (p, s) => { setCurrentPage(p); setPageSize(s); },
            showSizeChanger: true,
            showTotal: (total) => (
              <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>
                {hasMore
                  ? `${(currentPage - 1) * pageSize + 1}-${currentPage * pageSize}+`
                  : `${total} users`}
              </span>
            ),
            style: { padding: '8px 16px' },
          }}
        />
      </div>

      {/* Role Modal */}
      <Modal
        title={`Change Role â€” ${editingUser?.username}`}
        open={isRoleModalVisible}
        onCancel={() => setIsRoleModalVisible(false)}
        footer={null}
        width={360}
      >
        <Form form={roleForm} layout="vertical" onFinish={handleUpdateRole} style={{ marginTop: 16 }}>
          <Form.Item name="role" label="Role" rules={[{ required: true }]}>
            <Select>
              <Select.Option value="User">User</Select.Option>
              <Select.Option value="Admin">Admin</Select.Option>
              <Select.Option value="SuperAdmin">SuperAdmin</Select.Option>
            </Select>
          </Form.Item>
          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => setIsRoleModalVisible(false)}>Cancel</Button>
              <Button type="primary" htmlType="submit">Save</Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      {/* User Detail Drawer */}
      <UserDetailDrawer
        userId={drawerUserId}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
      />
    </div>
  );
}
