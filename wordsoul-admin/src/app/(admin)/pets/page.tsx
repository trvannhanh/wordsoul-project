'use client';

import React, { useState, useEffect } from 'react';
import {
  Table, Input, Select, Button, Modal, Form,
  App, Popconfirm, Space, Avatar, Tag, Upload,
  InputNumber, Row, Col,
} from 'antd';
import {
  SearchOutlined, EditOutlined, DeleteOutlined,
  PlusOutlined, UploadOutlined, HeartOutlined,
} from '@ant-design/icons';
import type { UploadFile } from 'antd';
import { authApi, endpoints } from '@/services/api';

// ── Types ────────────────────────────────────────────────────────────────────
interface Pet {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  rarity: string;
  type: string;
  secondaryType?: string;
  baseFormId?: number;
  nextEvolutionId?: number;
  requiredLevel?: number;
  isOwned?: boolean;
}

// ── Constants ─────────────────────────────────────────────────────────────────
const RARITIES = ['Common', 'Uncommon', 'Rare', 'Epic', 'Legendary'];
const PET_TYPES = [
  'Normal', 'Fire', 'Water', 'Electric', 'Grass', 'Ice',
  'Fighting', 'Poison', 'Ground', 'Flying', 'Psychic', 'Bug',
  'Rock', 'Ghost', 'Dragon', 'Dark', 'Steel', 'Fairy',
];

const RARITY_COLORS: Record<string, string> = {
  Common:    'default',
  Uncommon:  'success',
  Rare:      'processing',
  Epic:      'purple',
  Legendary: 'gold',
};

const TYPE_COLORS: Record<string, string> = {
  Normal: 'default', Fire: 'orange', Water: 'blue', Electric: 'gold',
  Grass: 'green', Ice: 'cyan', Fighting: 'red', Poison: 'purple',
  Ground: 'brown', Flying: 'geekblue', Psychic: 'magenta', Bug: 'lime',
  Rock: 'volcano', Ghost: 'purple', Dragon: 'geekblue', Dark: 'default',
  Steel: 'default', Fairy: 'pink',
};

// ── Main Page ─────────────────────────────────────────────────────────────────
export default function PetsPage() {
  const { message } = App.useApp();

  const [pets, setPets] = useState<Pet[]>([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  // Filters
  const [search, setSearch] = useState('');
  const [filterRarity, setFilterRarity] = useState<string | undefined>();
  const [filterType, setFilterType] = useState<string | undefined>();

  // Modal
  const [modalOpen, setModalOpen] = useState(false);
  const [editingPet, setEditingPet] = useState<Pet | null>(null);
  const [form] = Form.useForm();
  const [fileList, setFileList] = useState<UploadFile[]>([]);

  // ── Fetch ──────────────────────────────────────────────────────────────────
  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      try {
        const params: Record<string, unknown> = { pageSize: 500 };
        if (search) params.name = search;
        if (filterRarity !== undefined) params.rarity = RARITIES.indexOf(filterRarity);
        if (filterType !== undefined) params.type = PET_TYPES.indexOf(filterType);

        const res = await authApi.get<Pet[]>(endpoints.pets, { params });
        if (!cancelled) setPets(res.data);
      } catch {
        if (!cancelled) message.error('Failed to load pets');
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    load();
    return () => { cancelled = true; };
  }, [search, filterRarity, filterType, message]);

  const refresh = async () => {
    setLoading(true);
    try {
      const params: Record<string, unknown> = { pageSize: 500 };
      if (search) params.name = search;
      if (filterRarity !== undefined) params.rarity = RARITIES.indexOf(filterRarity);
      if (filterType !== undefined) params.type = PET_TYPES.indexOf(filterType);
      const res = await authApi.get<Pet[]>(endpoints.pets, { params });
      setPets(res.data);
    } catch {
      message.error('Failed to load pets');
    } finally {
      setLoading(false);
    }
  };

  // ── Open modal ─────────────────────────────────────────────────────────────
  const openCreate = () => {
    setEditingPet(null);
    form.resetFields();
    setFileList([]);
    setModalOpen(true);
  };

  const openEdit = (pet: Pet) => {
    setEditingPet(pet);
    form.setFieldsValue({
      name: pet.name,
      description: pet.description,
      rarity: pet.rarity,
      type: pet.type,
      secondaryType: pet.secondaryType,
      requiredLevel: pet.requiredLevel,
      baseFormId: pet.baseFormId,
      nextEvolutionId: pet.nextEvolutionId,
    });
    setFileList(
      pet.imageUrl
        ? [{ uid: '-1', name: 'current', status: 'done', url: pet.imageUrl }]
        : [],
    );
    setModalOpen(true);
  };

  // ── Save (Create or Edit) ──────────────────────────────────────────────────
  const handleSave = async () => {
    try {
      await form.validateFields();
    } catch {
      return;
    }

    const values = form.getFieldsValue();
    const formData = new FormData();
    formData.append('Name', values.name);
    formData.append('Description', values.description ?? '');
    formData.append('Rarity', String(RARITIES.indexOf(values.rarity)));
    formData.append('Type', String(PET_TYPES.indexOf(values.type)));
    if (values.secondaryType)
      formData.append('SecondaryType', String(PET_TYPES.indexOf(values.secondaryType)));
    if (values.requiredLevel != null)
      formData.append('RequiredLevel', String(values.requiredLevel));
    if (values.baseFormId != null)
      formData.append('BaseFormId', String(values.baseFormId));
    if (values.nextEvolutionId != null)
      formData.append('NextEvolutionId', String(values.nextEvolutionId));

    // Attach new image file if selected
    const newFile = fileList.find(f => f.originFileObj);
    if (newFile?.originFileObj) {
      formData.append('ImageFile', newFile.originFileObj);
    }

    setSaving(true);
    try {
      if (editingPet) {
        await authApi.put(endpoints.petDetail(editingPet.id), formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
        message.success('Pet updated');
      } else {
        await authApi.post(endpoints.pets, formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
        message.success('Pet created');
      }
      setModalOpen(false);
      refresh();
    } catch {
      message.error('Failed to save pet');
    } finally {
      setSaving(false);
    }
  };

  // ── Delete ─────────────────────────────────────────────────────────────────────────────
  const handleDelete = async (id: number) => {
    try {
      await authApi.delete(endpoints.petDetail(id));
      message.success('Pet deleted');
      refresh();
    } catch {
      message.error('Failed to delete pet');
    }
  };

  // ── Columns ────────────────────────────────────────────────────────────────
  const columns = [
    {
      title: '',
      key: 'image',
      width: 52,
      render: (_: unknown, pet: Pet) => (
        <Avatar
          src={pet.imageUrl}
          size={36}
          icon={<HeartOutlined />}
          style={{ background: 'var(--bg-muted)' }}
        />
      ),
    },
    {
      title: 'Name',
      dataIndex: 'name',
      render: (name: string) => <span style={{ fontWeight: 500 }}>{name}</span>,
    },
    {
      title: 'Rarity',
      dataIndex: 'rarity',
      render: (r: string) => <Tag color={RARITY_COLORS[r] ?? 'default'}>{r}</Tag>,
      width: 100,
    },
    {
      title: 'Type',
      key: 'types',
      width: 160,
      render: (_: unknown, pet: Pet) => (
        <Space size={4}>
          <Tag color={TYPE_COLORS[pet.type] ?? 'default'}>{pet.type}</Tag>
          {pet.secondaryType && (
            <Tag color={TYPE_COLORS[pet.secondaryType] ?? 'default'}>{pet.secondaryType}</Tag>
          )}
        </Space>
      ),
    },
    {
      title: 'Req. Level',
      dataIndex: 'requiredLevel',
      width: 100,
      render: (v: number | undefined) => v ?? <span style={{ color: 'var(--text-muted)' }}>—</span>,
    },
    {
      title: 'Base Form',
      dataIndex: 'baseFormId',
      width: 90,
      render: (v: number | undefined) => v ?? <span style={{ color: 'var(--text-muted)' }}>—</span>,
    },
    {
      title: 'Next Evo',
      dataIndex: 'nextEvolutionId',
      width: 90,
      render: (v: number | undefined) => v ?? <span style={{ color: 'var(--text-muted)' }}>—</span>,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_: unknown, pet: Pet) => (
        <Space size={4}>
          <Button
            type="text"
            size="small"
            icon={<EditOutlined />}
            onClick={() => openEdit(pet)}
          />
          <Popconfirm
            title="Delete this pet?"
            description={`"${pet.name}" will be permanently removed.`}
            onConfirm={() => handleDelete(pet.id)}
            okText="Delete"
            okButtonProps={{ danger: true }}
          >
            <Button type="text" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // ── Render ─────────────────────────────────────────────────────────────────
  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Pet Management</h1>
          <p className="page-subtitle">Create, edit and remove pets from the platform.</p>
        </div>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
          Create Pet
        </Button>
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: 8, marginBottom: 16, flexWrap: 'wrap' }}>
        <Input
          prefix={<SearchOutlined />}
          placeholder="Search by name…"
          value={search}
          onChange={e => setSearch(e.target.value)}
          style={{ width: 220 }}
          allowClear
        />
        <Select
          placeholder="Rarity"
          value={filterRarity}
          onChange={v => setFilterRarity(v)}
          allowClear
          style={{ width: 140 }}
        >
          {RARITIES.map(r => (
            <Select.Option key={r} value={r}>
              <Tag color={RARITY_COLORS[r]}>{r}</Tag>
            </Select.Option>
          ))}
        </Select>
        <Select
          placeholder="Type"
          value={filterType}
          onChange={v => setFilterType(v)}
          allowClear
          style={{ width: 140 }}
        >
          {PET_TYPES.map(t => (
            <Select.Option key={t} value={t}>{t}</Select.Option>
          ))}
        </Select>
      </div>

      {/* Table */}
      <Table
        dataSource={pets}
        columns={columns}
        rowKey="id"
        loading={loading}
        size="small"
        pagination={{
          pageSize: 20,
          showSizeChanger: false,
          showTotal: total => `${total} pets`,
        }}
        style={{ background: 'var(--bg-surface)' }}
      />

      {/* Create / Edit Modal */}
      <Modal
        open={modalOpen}
        title={editingPet ? `Edit — ${editingPet.name}` : 'Create Pet'}
        onCancel={() => setModalOpen(false)}
        onOk={handleSave}
        okText={editingPet ? 'Save' : 'Create'}
        confirmLoading={saving}
        width={560}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Row gutter={12}>
            <Col span={16}>
              <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Required' }]}>
                <Input placeholder="e.g. Flamequill" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="requiredLevel" label="Req. Level">
                <InputNumber min={1} style={{ width: '100%' }} placeholder="1" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item name="description" label="Description">
            <Input.TextArea rows={2} placeholder="Short description…" />
          </Form.Item>

          <Row gutter={12}>
            <Col span={8}>
              <Form.Item name="rarity" label="Rarity" rules={[{ required: true, message: 'Required' }]}>
                <Select placeholder="Select">
                  {RARITIES.map(r => (
                    <Select.Option key={r} value={r}>
                      <Tag color={RARITY_COLORS[r]}>{r}</Tag>
                    </Select.Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="type" label="Type" rules={[{ required: true, message: 'Required' }]}>
                <Select placeholder="Primary type" showSearch>
                  {PET_TYPES.map(t => <Select.Option key={t} value={t}>{t}</Select.Option>)}
                </Select>
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="secondaryType" label="Secondary Type">
                <Select placeholder="Optional" allowClear showSearch>
                  {PET_TYPES.map(t => <Select.Option key={t} value={t}>{t}</Select.Option>)}
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={12}>
            <Col span={12}>
              <Form.Item name="baseFormId" label="Base Form ID">
                <InputNumber min={1} style={{ width: '100%' }} placeholder="ID of base form" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="nextEvolutionId" label="Next Evolution ID">
                <InputNumber min={1} style={{ width: '100%' }} placeholder="ID of next evo" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item label="Image">
            <Upload
              listType="picture"
              fileList={fileList}
              maxCount={1}
              beforeUpload={() => false}
              onChange={({ fileList: fl }) => setFileList(fl)}
              accept="image/*"
            >
              <Button icon={<UploadOutlined />}>Upload Image</Button>
            </Upload>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
