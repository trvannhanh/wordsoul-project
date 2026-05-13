'use client';

import React, { useState, useEffect, useCallback } from 'react';
import {
  Table, Input, Select, Button, Tag, Modal,
  Form, App, Popconfirm, Space, Avatar,
} from 'antd';
import {
  SearchOutlined, EditOutlined, DeleteOutlined,
  UserAddOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';
import dayjs from 'dayjs';

interface User {
  id: number;
  username: string;
  email: string;
  role: string;
  createdAt: string;
  isActive: boolean;
}

const ROLE_STYLE: Record<string, { color: string; bg: string; border: string }> = {
  SuperAdmin: { color: 'var(--accent)',   bg: 'var(--accent-subtle)',  border: 'var(--accent-border)' },
  Admin:      { color: '#0369A1',         bg: '#F0F9FF',               border: '#BAE6FD' },
  User:       { color: 'var(--text-muted)', bg: 'var(--bg-muted)',     border: 'var(--border)' },
};

function RoleBadge({ role }: { role: string }) {
  const s = ROLE_STYLE[role] ?? ROLE_STYLE['User'];
  return (
    <span
      style={{
        fontSize: 11,
        fontWeight: 600,
        padding: '2px 7px',
        borderRadius: 4,
        color: s.color,
        background: s.bg,
        border: `1px solid ${s.border}`,
        whiteSpace: 'nowrap',
      }}
    >
      {role || 'User'}
    </span>
  );
}

function StatusBadge({ isActive }: { isActive: boolean }) {
  return (
    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 5 }}>
      <span
        className={`status-dot ${isActive ? 'healthy' : 'error'}`}
        style={{
          display: 'inline-block',
          width: 6,
          height: 6,
          borderRadius: '50%',
          background: isActive ? 'var(--success)' : 'var(--danger)',
        }}
      />
      <span style={{ fontSize: 12, color: isActive ? 'var(--success)' : 'var(--danger)', fontWeight: 500 }}>
        {isActive ? 'Active' : 'Banned'}
      </span>
    </span>
  );
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

  const [isRoleModalVisible, setIsRoleModalVisible] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [roleForm] = Form.useForm();

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

  const handleDeleteUser = async (userId: number) => {
    try {
      await authApi.delete(`${endpoints.users}/${userId}`);
      message.success('User deleted');
      fetchUsers(currentPage, pageSize, searchText, roleFilter);
    } catch { message.error('Failed to delete user'); }
  };

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
    } catch { message.error('Failed to update role'); }
  };

  const isSuperAdmin = currentUser?.role === 'SuperAdmin';

  const columns = [
    {
      title: 'User',
      key: 'user',
      render: (_: any, record: User) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
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
      width: 100,
      render: (_: any, record: User) => (
        <Space size={0}>
          <Button
            type="text"
            size="small"
            icon={<EditOutlined />}
            onClick={() => openRoleModal(record)}
            disabled={!isSuperAdmin || record.id.toString() === currentUser?.id}
            style={{ color: 'var(--text-muted)', fontSize: 13 }}
          >
            Role
          </Button>
          <Popconfirm
            title="Delete this user?"
            description="This action cannot be undone."
            onConfirm={() => handleDeleteUser(record.id)}
            okText="Delete"
            okButtonProps={{ danger: true }}
            cancelText="Cancel"
            disabled={
              record.id.toString() === currentUser?.id ||
              (record.role === 'SuperAdmin' && !isSuperAdmin)
            }
          >
            <Button
              type="text"
              size="small"
              danger
              icon={<DeleteOutlined />}
              disabled={
                record.id.toString() === currentUser?.id ||
                (record.role === 'SuperAdmin' && !isSuperAdmin)
              }
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">User Management</h1>
          <p className="page-subtitle">View, filter, and manage platform user accounts and roles.</p>
        </div>
        <Button type="primary" size="small" icon={<UserAddOutlined />}>
          Add User
        </Button>
      </div>

      {/* Main Card */}
      <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
        {/* Filter Bar */}
        <div
          style={{
            padding: '12px 16px',
            borderBottom: '1px solid var(--border)',
            display: 'flex',
            alignItems: 'center',
            gap: 8,
            flexWrap: 'wrap',
          }}
        >
          <Input.Search
            placeholder="Search by username or email…"
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
        </div>

        {/* Table */}
        <Table
          columns={columns}
          dataSource={users}
          rowKey="id"
          loading={loading}
          size="small"
          style={{ fontSize: 13 }}
          pagination={{
            current: currentPage,
            pageSize,
            total: hasMore ? currentPage * pageSize + 1 : currentPage * pageSize,
            onChange: (p, s) => { setCurrentPage(p); setPageSize(s); },
            showSizeChanger: true,
            showTotal: (total) => (
              <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>
                {hasMore ? `${(currentPage - 1) * pageSize + 1}–${currentPage * pageSize}+` : `${total} users`}
              </span>
            ),
            style: { padding: '8px 16px' },
          }}
        />
      </div>

      {/* Role Modal */}
      <Modal
        title={`Change Role — ${editingUser?.username}`}
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
    </div>
  );
}
