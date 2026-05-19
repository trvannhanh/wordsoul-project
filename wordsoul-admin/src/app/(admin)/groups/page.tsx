'use client';

import React, { useState, useCallback, useEffect } from 'react';
import {
  Table, Button, Input, Space, Typography, Tag, Tooltip, Popconfirm,
  Modal, Form, Drawer, List, Avatar, message, Divider, Select,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  PlusOutlined, SearchOutlined, EditOutlined, DeleteOutlined,
  TeamOutlined, UserAddOutlined, UserDeleteOutlined, ReloadOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

const { Title, Text } = Typography;

// ─── Types ────────────────────────────────────────────────────────────────────

interface GroupMember {
  userId: number;
  username: string;
  email: string;
  avatarUrl?: string;
  joinedAt: string;
}

interface UserGroup {
  id: number;
  name: string;
  description?: string;
  memberCount: number;
  createdByUserId: number;
  createdByUsername?: string;
  createdAt: string;
}

interface UserGroupDetail extends UserGroup {
  members: GroupMember[];
}

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

interface UserOption {
  id: number;
  username: string;
  email: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function GroupsPage() {
  const [groups, setGroups] = useState<UserGroup[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(15);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');

  // Create / Edit modal
  const [modalOpen, setModalOpen] = useState(false);
  const [editingGroup, setEditingGroup] = useState<UserGroup | null>(null);
  const [formSubmitting, setFormSubmitting] = useState(false);
  const [form] = Form.useForm();

  // Member drawer
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [drawerGroup, setDrawerGroup] = useState<UserGroupDetail | null>(null);
  const [drawerLoading, setDrawerLoading] = useState(false);
  const [addMemberUserId, setAddMemberUserId] = useState<number | null>(null);
  const [userOptions, setUserOptions] = useState<UserOption[]>([]);
  const [userSearchLoading, setUserSearchLoading] = useState(false);

  // ─── Fetch helpers ──────────────────────────────────────────────────────────

  const fetchGroups = useCallback(async () => {
    setLoading(true);
    try {
      const params: Record<string, unknown> = { pageNumber, pageSize };
      if (search) params.search = search;
      const { data } = await authApi.get<PagedResult<UserGroup>>(endpoints.adminGroups, { params });
      setGroups(data.items ?? []);
      setTotal(data.totalCount ?? 0);
    } catch {
      message.error('Failed to load groups');
    } finally {
      setLoading(false);
    }
  }, [pageNumber, pageSize, search]);

  useEffect(() => { fetchGroups(); }, [fetchGroups]);

  const fetchGroupDetail = async (id: number) => {
    setDrawerLoading(true);
    try {
      const { data } = await authApi.get<UserGroupDetail>(endpoints.adminGroupDetail(id));
      setDrawerGroup(data);
    } catch {
      message.error('Failed to load group details');
    } finally {
      setDrawerLoading(false);
    }
  };

  const searchUsers = async (keyword: string) => {
    if (!keyword || keyword.length < 2) { setUserOptions([]); return; }
    setUserSearchLoading(true);
    try {
      const { data } = await authApi.get<PagedResult<UserOption>>(endpoints.users, {
        params: { pageNumber: 1, pageSize: 20, search: keyword },
      });
      setUserOptions(data.items ?? []);
    } catch {
      setUserOptions([]);
    } finally {
      setUserSearchLoading(false);
    }
  };

  // ─── Handlers ───────────────────────────────────────────────────────────────

  const openCreateModal = () => {
    setEditingGroup(null);
    form.resetFields();
    setModalOpen(true);
  };

  const openEditModal = (group: UserGroup) => {
    setEditingGroup(group);
    form.setFieldsValue({ name: group.name, description: group.description });
    setModalOpen(true);
  };

  const handleModalSubmit = async () => {
    let values: { name: string; description?: string };
    try { values = await form.validateFields(); } catch { return; }

    setFormSubmitting(true);
    try {
      if (editingGroup) {
        await authApi.put(endpoints.adminGroupDetail(editingGroup.id), values);
        message.success('Group updated');
      } else {
        await authApi.post(endpoints.adminGroups, values);
        message.success('Group created');
      }
      setModalOpen(false);
      fetchGroups();
    } catch {
      message.error(editingGroup ? 'Update failed' : 'Create failed');
    } finally {
      setFormSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await authApi.delete(endpoints.adminGroupDetail(id));
      message.success('Group deleted');
      fetchGroups();
    } catch {
      message.error('Delete failed');
    }
  };

  const openDrawer = async (group: UserGroup) => {
    setDrawerOpen(true);
    setDrawerGroup(null);
    setAddMemberUserId(null);
    setUserOptions([]);
    await fetchGroupDetail(group.id);
  };

  const handleAddMember = async () => {
    if (!drawerGroup || !addMemberUserId) return;
    try {
      await authApi.post(endpoints.adminGroupMembers(drawerGroup.id), { userId: addMemberUserId });
      message.success('Member added');
      setAddMemberUserId(null);
      setUserOptions([]);
      await fetchGroupDetail(drawerGroup.id);
    } catch {
      message.error('Failed to add member');
    }
  };

  const handleRemoveMember = async (userId: number) => {
    if (!drawerGroup) return;
    try {
      await authApi.delete(endpoints.adminGroupMember(drawerGroup.id, userId));
      message.success('Member removed');
      await fetchGroupDetail(drawerGroup.id);
    } catch {
      message.error('Failed to remove member');
    }
  };

  // ─── Table columns ──────────────────────────────────────────────────────────

  const columns: ColumnsType<UserGroup> = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record) => (
        <Button type="link" onClick={() => openDrawer(record)} style={{ padding: 0 }}>
          {name}
        </Button>
      ),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (desc?: string) => desc ? <Text type="secondary">{desc}</Text> : <Text type="secondary" italic>—</Text>,
    },
    {
      title: 'Members',
      dataIndex: 'memberCount',
      key: 'memberCount',
      width: 100,
      render: (count: number) => <Tag icon={<TeamOutlined />} color="blue">{count}</Tag>,
    },
    {
      title: 'Created By',
      dataIndex: 'createdByUsername',
      key: 'createdByUsername',
      width: 140,
      render: (name?: string) => <Text>{name ?? '—'}</Text>,
    },
    {
      title: 'Created At',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 160,
      render: (date: string) => new Date(date).toLocaleDateString('vi-VN'),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 110,
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Edit">
            <Button size="small" icon={<EditOutlined />} onClick={() => openEditModal(record)} />
          </Tooltip>
          <Tooltip title="Delete">
            <Popconfirm
              title={`Delete group "${record.name}"?`}
              description="This will remove the group and all its members."
              okText="Delete"
              okButtonProps={{ danger: true }}
              onConfirm={() => handleDelete(record.id)}
            >
              <Button size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          </Tooltip>
        </Space>
      ),
    },
  ];

  // ─── Render ─────────────────────────────────────────────────────────────────

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Space>
          <Input
            placeholder="Search groups..."
            prefix={<SearchOutlined />}
            value={searchInput}
            onChange={e => setSearchInput(e.target.value)}
            onPressEnter={() => { setSearch(searchInput); setPageNumber(1); }}
            allowClear
            onClear={() => { setSearchInput(''); setSearch(''); setPageNumber(1); }}
            style={{ width: 260 }}
          />
          <Button icon={<ReloadOutlined />} onClick={fetchGroups} />
        </Space>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreateModal}>
          New Group
        </Button>
      </div>

      <Table
        rowKey="id"
        columns={columns}
        dataSource={groups}
        loading={loading}
        pagination={{
          current: pageNumber,
          pageSize,
          total,
          showSizeChanger: true,
          showTotal: (t) => `${t} groups`,
          onChange: (page, size) => { setPageNumber(page); setPageSize(size); },
        }}
      />

      {/* Create / Edit modal */}
      <Modal
        title={editingGroup ? 'Edit Group' : 'Create Group'}
        open={modalOpen}
        onOk={handleModalSubmit}
        onCancel={() => setModalOpen(false)}
        confirmLoading={formSubmitting}
        okText={editingGroup ? 'Save' : 'Create'}
        destroyOnClose
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            name="name"
            label="Group Name"
            rules={[{ required: true, message: 'Please enter a group name' }, { max: 100 }]}
          >
            <Input placeholder="e.g. Beta Testers" />
          </Form.Item>
          <Form.Item name="description" label="Description" rules={[{ max: 500 }]}>
            <Input.TextArea rows={3} placeholder="Optional description..." />
          </Form.Item>
        </Form>
      </Modal>

      {/* Member management drawer */}
      <Drawer
        title={drawerGroup ? `Members: ${drawerGroup.name}` : 'Group Members'}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        width={480}
        loading={drawerLoading}
      >
        {drawerGroup && (
          <>
            {drawerGroup.description && (
              <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
                {drawerGroup.description}
              </Text>
            )}

            <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
              <Select
                showSearch
                style={{ flex: 1 }}
                placeholder="Search user to add..."
                filterOption={false}
                onSearch={searchUsers}
                loading={userSearchLoading}
                value={addMemberUserId}
                onChange={val => setAddMemberUserId(val)}
                notFoundContent={userSearchLoading ? 'Searching...' : 'No users found'}
                options={userOptions.map(u => ({
                  value: u.id,
                  label: `${u.username} (${u.email})`,
                }))}
              />
              <Button
                type="primary"
                icon={<UserAddOutlined />}
                disabled={!addMemberUserId}
                onClick={handleAddMember}
              >
                Add
              </Button>
            </div>

            <Divider style={{ marginTop: 0 }}>
              <Text type="secondary">{drawerGroup.memberCount} member{drawerGroup.memberCount !== 1 ? 's' : ''}</Text>
            </Divider>

            <List
              dataSource={drawerGroup.members}
              renderItem={member => (
                <List.Item
                  actions={[
                    <Popconfirm
                      key="remove"
                      title="Remove this member?"
                      okText="Remove"
                      okButtonProps={{ danger: true }}
                      onConfirm={() => handleRemoveMember(member.userId)}
                    >
                      <Button size="small" type="text" danger icon={<UserDeleteOutlined />} />
                    </Popconfirm>,
                  ]}
                >
                  <List.Item.Meta
                    avatar={
                      <Avatar src={member.avatarUrl} style={{ background: '#1677ff' }}>
                        {!member.avatarUrl && member.username[0]?.toUpperCase()}
                      </Avatar>
                    }
                    title={member.username}
                    description={
                      <Space direction="vertical" size={0}>
                        <Text type="secondary" style={{ fontSize: 12 }}>{member.email}</Text>
                        <Text type="secondary" style={{ fontSize: 11 }}>
                          Joined {new Date(member.joinedAt).toLocaleDateString('vi-VN')}
                        </Text>
                      </Space>
                    }
                  />
                </List.Item>
              )}
            />
          </>
        )}
      </Drawer>
    </div>
  );
}
