'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { Table, Button, Select, Modal, Form, Input, InputNumber, Switch, App, Space, Popconfirm } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

const QUEST_TYPE_STYLE: Record<string, { color: string; bg: string }> = {
  Learn:    { color: '#0369A1', bg: '#F0F9FF' },
  Review:   { color: '#6D28D9', bg: '#F5F3FF' },
  Accuracy: { color: '#B45309', bg: '#FFFBEB' },
  Catch:    { color: '#15803D', bg: '#F0FDF4' },
};

function TypeBadge({ type }: { type: string }) {
  const s = QUEST_TYPE_STYLE[type] ?? { color: 'var(--text-muted)', bg: 'var(--bg-muted)' };
  return (
    <span style={{ fontSize: 11, fontWeight: 600, padding: '2px 7px', borderRadius: 4, color: s.color, background: s.bg }}>
      {type}
    </span>
  );
}

type TabKey = 'quests' | 'achievements';

export default function QuestsPage() {
  const [activeTab, setActiveTab] = useState<TabKey>('quests');
  const [quests, setQuests] = useState([]);
  const [achievements, setAchievements] = useState([]);
  const [loading, setLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingQuest, setEditingQuest] = useState<any>(null);
  const [editingAchievement, setEditingAchievement] = useState<any>(null);
  const [rewardTypeValue, setRewardTypeValue] = useState<string>('XP');
  const [form] = Form.useForm();
  const { message } = App.useApp();

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      if (activeTab === 'quests') {
        const res = await authApi.get(endpoints.adminQuests);
        setQuests(res.data);
      } else {
        const res = await authApi.get(endpoints.adminAchievements);
        setAchievements(res.data);
      }
    } catch { message.error('Failed to fetch data'); }
    finally { setLoading(false); }
  }, [activeTab, message]);

  useEffect(() => { fetchData(); }, [fetchData]);

  const openCreate = () => {
    setEditingQuest(null);
    setEditingAchievement(null);
    form.resetFields();
    setRewardTypeValue('XP');
    setIsModalOpen(true);
  };

  const openEditQuest = (record: any) => {
    setEditingQuest(record);
    setEditingAchievement(null);
    setRewardTypeValue(record.rewardType ?? 'XP');
    form.setFieldsValue({
      title: record.title,
      description: record.description,
      questType: record.questType,
      targetValue: record.targetValue,
      rewardType: record.rewardType,
      rewardValue: record.rewardValue,
      rewardReferenceId: record.rewardReferenceId,
      isActive: record.isActive,
    });
    setIsModalOpen(true);
  };

  const openEditAchievement = (record: any) => {
    setEditingAchievement(record);
    setEditingQuest(null);
    form.setFieldsValue({
      name: record.name,
      description: record.description,
      conditionType: record.conditionType,
      conditionValue: record.conditionValue,
      rewardItemId: record.rewardItemId,
      rewardXp: record.rewardXp,
    });
    setIsModalOpen(true);
  };

  const handleSubmit = async (values: any) => {
    try {
      if (activeTab === 'quests') {
        if (editingQuest) {
          await authApi.put(`${endpoints.adminQuests}/${editingQuest.id}`, values);
          message.success('Quest updated');
        } else {
          await authApi.post(endpoints.adminQuests, values);
          message.success('Quest created');
        }
      } else {
        if (editingAchievement) {
          await authApi.put(`${endpoints.adminAchievements}/${editingAchievement.id}`, values);
          message.success('Achievement updated');
        } else {
          await authApi.post(endpoints.adminAchievements, values);
          message.success('Achievement created');
        }
      }
      setIsModalOpen(false);
      form.resetFields();
      fetchData();
    } catch { message.error('Failed to save'); }
  };

  const handleDeleteQuest = async (id: number) => {
    try {
      await authApi.delete(`${endpoints.adminQuests}/${id}`);
      message.success('Quest deleted');
      fetchData();
    } catch { message.error('Failed to delete quest'); }
  };

  const handleDeleteAchievement = async (id: number) => {
    try {
      await authApi.delete(`${endpoints.adminAchievements}/${id}`);
      message.success('Achievement deleted');
      fetchData();
    } catch { message.error('Failed to delete achievement'); }
  };

  const toggleQuest = async (id: number) => {
    try {
      await authApi.patch(`${endpoints.adminQuests}/${id}/toggle`);
      fetchData();
    } catch { message.error('Failed to update'); }
  };

  const questColumns = [
    { title: 'Quest Title', dataIndex: 'title', key: 'title', render: (v: string) => <span style={{ fontWeight: 500, fontSize: 13 }}>{v}</span> },
    { title: 'Type', dataIndex: 'questType', key: 'questType', width: 110, render: (v: string) => <TypeBadge type={v} /> },
    { title: 'Target', dataIndex: 'targetValue', key: 'targetValue', width: 70, render: (v: number) => <span style={{ fontSize: 12, color: 'var(--text-muted)', fontVariantNumeric: 'tabular-nums' }}>{v}</span> },
    { title: 'Reward Type', dataIndex: 'rewardType', key: 'rewardType', width: 100, render: (v: string) => <TypeBadge type={v} /> },
    { title: 'Reward', dataIndex: 'rewardValue', key: 'rewardValue', width: 80, render: (v: number) => <span style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}>{v}</span> },
    { title: 'Ref ID', dataIndex: 'rewardReferenceId', key: 'rewardReferenceId', width: 70, render: (v: number) => <span style={{ fontSize: 12 }}>{v ?? 'â€”'}</span> },
    {
      title: 'Active',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 70,
      render: (v: boolean, record: any) => (
        <Switch checked={v} size="small" onChange={() => toggleQuest(record.id)} />
      ),
    },
    {
      title: '',
      key: 'actions',
      width: 90,
      render: (_: any, record: any) => (
        <Space size={4}>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEditQuest(record)} />
          <Popconfirm title="Delete this quest?" okText="Delete" okButtonProps={{ danger: true }} onConfirm={() => handleDeleteQuest(record.id)}>
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const achievementColumns = [
    { title: 'Achievement', dataIndex: 'name', key: 'name', render: (v: string) => <span style={{ fontWeight: 500, fontSize: 13 }}>{v}</span> },
    { title: 'Description', dataIndex: 'description', key: 'description', render: (v: string) => <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{v}</span> },
    { title: 'Condition', dataIndex: 'conditionType', key: 'conditionType', width: 140, render: (v: string) => <TypeBadge type={v} /> },
    { title: 'Target', dataIndex: 'conditionValue', key: 'conditionValue', width: 70, render: (v: number) => <span style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}>{v}</span> },
    { title: 'XP', dataIndex: 'rewardXp', key: 'rewardXp', width: 70, render: (v: number) => <span style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}>{v ?? 'â€”'}</span> },
    { title: 'Item ID', dataIndex: 'rewardItemId', key: 'rewardItemId', width: 70, render: (v: number) => <span style={{ fontSize: 12 }}>{v ?? 'â€”'}</span> },
    {
      title: '',
      key: 'actions',
      width: 90,
      render: (_: any, record: any) => (
        <Space size={4}>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEditAchievement(record)} />
          <Popconfirm title="Delete this achievement?" okText="Delete" okButtonProps={{ danger: true }} onConfirm={() => handleDeleteAchievement(record.id)}>
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const tabs: { key: TabKey; label: string }[] = [
    { key: 'quests', label: 'Daily Quests' },
    { key: 'achievements', label: 'Achievements' },
  ];

  const isEditing = editingQuest || editingAchievement;
  const modalTitle = isEditing
    ? `Edit ${activeTab === 'quests' ? 'Quest' : 'Achievement'}`
    : `Create ${activeTab === 'quests' ? 'Quest' : 'Achievement'}`;

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Quests & Achievements</h1>
          <p className="page-subtitle">Configure daily quests and one-time achievement milestones.</p>
        </div>
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={openCreate}
        >
          Create {activeTab === 'quests' ? 'Quest' : 'Achievement'}
        </Button>
      </div>

      {/* Tabs */}
      <div style={{ display: 'flex', gap: 2, marginBottom: 12 }}>
        {tabs.map(tab => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            style={{
              padding: '5px 14px',
              borderRadius: 6,
              border: activeTab === tab.key ? '1px solid var(--accent-border)' : '1px solid var(--border)',
              background: activeTab === tab.key ? 'var(--accent-subtle)' : 'var(--bg-surface)',
              color: activeTab === tab.key ? 'var(--accent)' : 'var(--text-secondary)',
              fontWeight: activeTab === tab.key ? 600 : 400,
              fontSize: 13,
              cursor: 'pointer',
              transition: 'all 0.15s',
            }}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Table */}
      <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
        <Table
          dataSource={activeTab === 'quests' ? quests : achievements}
          columns={activeTab === 'quests' ? questColumns : achievementColumns}
          rowKey="id"
          loading={loading}
          size="small"
          pagination={{ pageSize: 15, style: { padding: '8px 16px' } }}
        />
      </div>

      {/* Create/Edit Modal */}
      <Modal
        title={modalTitle}
        open={isModalOpen}
        onCancel={() => { setIsModalOpen(false); form.resetFields(); setEditingQuest(null); setEditingAchievement(null); }}
        onOk={() => form.submit()}
        okText={isEditing ? 'Save' : 'Create'}
        width={420}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit} style={{ marginTop: 16 }}>
          {activeTab === 'quests' ? (
            <>
              <Form.Item name="title" label="Quest Title" rules={[{ required: true }]}><Input /></Form.Item>
              <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
              <Form.Item name="questType" label="Type" rules={[{ required: true }]}>
                <Select>
                  <Select.Option value="Learn">Learn Words</Select.Option>
                  <Select.Option value="Review">Review Words</Select.Option>
                  <Select.Option value="Accuracy">Accuracy</Select.Option>
                  <Select.Option value="Catch">Catch</Select.Option>
                </Select>
              </Form.Item>
              <Form.Item name="targetValue" label="Target Value" rules={[{ required: true }]}><InputNumber style={{ width: '100%' }} min={1} /></Form.Item>
              <Form.Item name="rewardType" label="Reward Type" rules={[{ required: true }]}>
                <Select onChange={(v) => setRewardTypeValue(v)}>
                  <Select.Option value="XP">XP</Select.Option>
                  <Select.Option value="AP">AP</Select.Option>
                  <Select.Option value="Item">Item</Select.Option>
                  <Select.Option value="Pet">Pet</Select.Option>
                </Select>
              </Form.Item>
              <Form.Item name="rewardValue" label="Reward Value" rules={[{ required: true }]}><InputNumber style={{ width: '100%' }} min={1} /></Form.Item>
              {(rewardTypeValue === 'Item' || rewardTypeValue === 'Pet') && (
                <Form.Item name="rewardReferenceId" label="Reward Reference ID (Item/Pet ID)" rules={[{ required: true }]}><InputNumber style={{ width: '100%' }} min={1} /></Form.Item>
              )}
              {isEditing && (
                <Form.Item name="isActive" label="Active" valuePropName="checked"><Switch /></Form.Item>
              )}
            </>
          ) : (
            <>
              <Form.Item name="name" label="Achievement Name" rules={[{ required: true }]}><Input /></Form.Item>
              <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
              <Form.Item name="conditionType" label="Condition Type" rules={[{ required: true }]}>
                <Select>
                  <Select.Option value="LevelReached">Level Reached</Select.Option>
                  <Select.Option value="TotalWordsLearned">Words Learned</Select.Option>
                  <Select.Option value="GymDefeated">Gym Defeated</Select.Option>
                  <Select.Option value="PvPWon">PvP Won</Select.Option>
                  <Select.Option value="LoginStreak">Login Streak</Select.Option>
                  <Select.Option value="PetsCaught">Pets Caught</Select.Option>
                </Select>
              </Form.Item>
              <Form.Item name="conditionValue" label="Target Value" rules={[{ required: true }]}><InputNumber style={{ width: '100%' }} min={1} /></Form.Item>
              <Form.Item name="rewardXp" label="XP Reward" initialValue={0}><InputNumber style={{ width: '100%' }} min={0} /></Form.Item>
              <Form.Item name="rewardItemId" label="Reward Item ID (optional)"><InputNumber style={{ width: '100%' }} min={1} /></Form.Item>
            </>
          )}
        </Form>
      </Modal>
    </div>
  );
}


