'use client';

import React, { useCallback, useEffect, useState } from 'react';
import {
  App, Avatar, Button, Col, Form, Image, Input, Modal,
  Popconfirm, Row, Select, Space, Table, Tag, Typography,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  DeleteOutlined, EditOutlined, GiftOutlined, PlusOutlined, SearchOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import ImageUploader from '../_components/ImageUploader';

const { Text } = Typography;

// ── Types ────────────────────────────────────────────────────────────────────
interface Item {
  id: number;
  name: string;
  description: string;
  imageUrl?: string;
  type: string;
}

// ── Constants ─────────────────────────────────────────────────────────────────
const ITEM_TYPES = ['Currency', 'Evolution', 'Booster'];

const TYPE_COLOR: Record<string, string> = {
  Currency:  'gold',
  Evolution: 'purple',
  Booster:   'blue',
};

const TYPE_ICON: Record<string, string> = {
  Currency:  '💰',
  Evolution: '🔮',
  Booster:   '⚡',
};

// ── Page ─────────────────────────────────────────────────────────────────────
export default function ItemsPage() {
  const { message } = App.useApp();

  const [items, setItems]     = useState<Item[]>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [search, setSearch]   = useState('');
  const [typeFilter, setTypeFilter] = useState<string | undefined>();

  // Modal state
  const [modalOpen, setModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [editingItem, setEditingItem] = useState<Item | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [form] = Form.useForm();

  // ── Fetch ──────────────────────────────────────────────────────────────────
  const loadItems = useCallback(async () => {
    setLoading(true);
    try {
      const res = await authApi.get(endpoints.items);
      setItems(res.data);
    } catch {
      message.error('Failed to load items.');
    } finally {
      setLoading(false);
    }
  }, [message]);

  useEffect(() => { loadItems(); }, [loadItems]);

  // ── Filtered data ──────────────────────────────────────────────────────────
  const filtered = items.filter((item) => {
    const matchSearch = !search || item.name.toLowerCase().includes(search.toLowerCase());
    const matchType   = !typeFilter || item.type === typeFilter;
    return matchSearch && matchType;
  });

  // ── Modal handlers ─────────────────────────────────────────────────────────
  function openCreate() {
    setModalMode('create');
    setEditingItem(null);
    setSelectedFile(null);
    form.resetFields();
    setModalOpen(true);
  }

  function openEdit(item: Item) {
    setModalMode('edit');
    setEditingItem(item);
    setSelectedFile(null);
    form.setFieldsValue({
      name:        item.name,
      description: item.description,
      type:        item.type,
    });
    setModalOpen(true);
  }

  async function handleSubmit() {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      if (modalMode === 'create') {
        const fd = new FormData();
        fd.append('name',        values.name);
        fd.append('description', values.description ?? '');
        fd.append('type',        values.type);
        if (selectedFile) fd.append('imageFile', selectedFile);

        await authApi.post(endpoints.items, fd, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
        message.success('Item created.');
      } else if (editingItem) {
        const fd = new FormData();
        fd.append('name',        values.name);
        fd.append('description', values.description ?? '');
        fd.append('type',        values.type);
        if (selectedFile) fd.append('imageFile', selectedFile);

        await authApi.put(endpoints.itemDetail(editingItem.id), fd, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
        message.success('Item updated.');
      }

      setModalOpen(false);
      loadItems();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'errorFields' in err) return; // validation
      message.error('Failed to save item.');
    } finally {
      setSubmitting(false);
    }
  }

  async function handleDelete(id: number) {
    try {
      await authApi.delete(endpoints.itemDetail(id));
      message.success('Item deleted.');
      setItems((prev) => prev.filter((i) => i.id !== id));
    } catch {
      message.error('Failed to delete item.');
    }
  }

  // ── Columns ────────────────────────────────────────────────────────────────
  const columns: ColumnsType<Item> = [
    {
      title: 'Image',
      dataIndex: 'imageUrl',
      key: 'imageUrl',
      width: 72,
      render: (url) =>
        url
          ? <Image src={url} width={44} height={44} style={{ borderRadius: 8, objectFit: 'cover' }} />
          : <Avatar icon={<GiftOutlined />} size={44} style={{ background: '#6366f1', borderRadius: 8 }} />,
    },
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (name) => <Text strong>{name}</Text>,
      sorter: (a, b) => a.name.localeCompare(b.name),
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      width: 130,
      render: (type) => (
        <Tag color={TYPE_COLOR[type] ?? 'default'}>
          {TYPE_ICON[type]} {type}
        </Tag>
      ),
      filters: ITEM_TYPES.map((t) => ({ text: t, value: t })),
      onFilter: (value, record) => record.type === value,
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (desc) => <Text type="secondary">{desc || '—'}</Text>,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 110,
      render: (_, record) => (
        <Space>
          <Button
            icon={<EditOutlined />}
            size="small"
            onClick={() => openEdit(record)}
          />
          <Popconfirm
            title="Delete this item?"
            description="This action cannot be undone."
            onConfirm={() => handleDelete(record.id)}
            okText="Delete"
            okButtonProps={{ danger: true }}
          >
            <Button icon={<DeleteOutlined />} size="small" danger />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // ── Render ─────────────────────────────────────────────────────────────────
  return (
    <div>
      {/* ── Toolbar ── */}
      <Row gutter={12} style={{ marginBottom: 16 }}>
        <Col flex="1">
          <Input
            placeholder="Search items…"
            prefix={<SearchOutlined />}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            allowClear
          />
        </Col>
        <Col>
          <Select
            placeholder="All types"
            allowClear
            value={typeFilter}
            onChange={setTypeFilter}
            style={{ width: 140 }}
            options={ITEM_TYPES.map((t) => ({ label: `${TYPE_ICON[t]} ${t}`, value: t }))}
          />
        </Col>
        <Col>
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
            Create Item
          </Button>
        </Col>
      </Row>

      {/* ── Table ── */}
      <Table<Item>
        columns={columns}
        dataSource={filtered}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 20, showTotal: (t) => `${t} items` }}
        size="middle"
      />

      {/* ── Create / Edit Modal ── */}
      <Modal
        title={modalMode === 'create' ? 'Create Item' : 'Edit Item'}
        open={modalOpen}
        onOk={handleSubmit}
        onCancel={() => setModalOpen(false)}
        okText={modalMode === 'create' ? 'Create' : 'Save'}
        confirmLoading={submitting}
        destroyOnHidden
        width={520}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 12 }}>
          {/* Image — only show uploader for create; edit shows existing + optional replacement */}
          <Form.Item label="Image">
            <ImageUploader
              value={modalMode === 'edit' ? editingItem?.imageUrl : undefined}
              onChange={(file) => setSelectedFile(file)}
            />
          </Form.Item>

          <Form.Item
            name="name"
            label="Name"
            rules={[{ required: true, message: 'Name is required' }, { max: 100 }]}
          >
            <Input placeholder="Item name" maxLength={100} showCount />
          </Form.Item>

          <Form.Item
            name="type"
            label="Type"
            rules={[{ required: true, message: 'Type is required' }]}
          >
            <Select
              placeholder="Select type"
              options={ITEM_TYPES.map((t) => ({
                label: <span>{TYPE_ICON[t]} {t}</span>,
                value: t,
              }))}
            />
          </Form.Item>

          <Form.Item name="description" label="Description" rules={[{ max: 300 }]}>
            <Input.TextArea
              placeholder="Item description"
              maxLength={300}
              showCount
              rows={3}
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
