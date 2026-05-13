'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { Table, Button, Select, Modal, Form, Input, InputNumber, Switch, App, Space } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

const QUEST_TYPE_STYLE: Record<string, { color: string; bg: string }> = {
  LearnWords:   { color: '#0369A1', bg: '#F0F9FF' },
  ReviewWords:  { color: '#6D28D9', bg: '#F5F3FF' },
  WinPvP:       { color: '#B45309', bg: '#FFFBEB' },
  PerfectScore: { color: '#15803D', bg: '#F0FDF4' },
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

  const handleCreate = async (values: any) => {
    try {
      if (activeTab === 'quests') {
        await authApi.post(endpoints.adminQuests, values);
        message.success('Quest created');
      } else {
        await authApi.post(endpoints.adminAchievements, values);
        message.success('Achievement created');
      }
      setIsModalOpen(false);
      form.resetFields();
      fetchData();
    } catch { message.error('Failed to create'); }
  };

  const toggleQuest = async (id: number) => {
    try {
      await authApi.patch(`${endpoints.adminQuests}/${id}/toggle`);
      fetchData();
    } catch { message.error('Failed to update'); }
  };

  const questColumns = [
    { title: 'Quest Title', dataIndex: 'title', key: 'title', render: (v: string) => <span style={{ fontWeight: 500, fontSize: 13 }}>{v}</span> },
    { title: 'Type', dataIndex: 'questType', key: 'questType', width: 130, render: (v: string) => <TypeBadge type={v} /> },
    { title: 'Target', dataIndex: 'targetCount', key: 'targetCount', width: 70, render: (v: number) => <span style={{ fontSize: 12, color: 'var(--text-muted)', fontVariantNumeric: 'tabular-nums' }}>{v}</span> },
    { title: 'XP Reward', dataIndex: 'rewardXp', key: 'rewardXp', width: 90, render: (v: number) => <span style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}>{v ?? '—'}</span> },
    { title: 'AP Reward', dataIndex: 'rewardAp', key: 'rewardAp', width: 90, render: (v: number) => <span style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}>{v ?? '—'}</span> },
    {
      title: 'Active',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 70,
      render: (v: boolean, record: any) => (
        <Switch checked={v} size="small" onChange={() => toggleQuest(record.id)} />
      ),
    },
  ];

  const achievementColumns = [
    { title: 'Achievement', dataIndex: 'name', key: 'name', render: (v: string) => <span style={{ fontWeight: 500, fontSize: 13 }}>{v}</span> },
    { title: 'Description', dataIndex: 'description', key: 'description', render: (v: string) => <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{v}</span> },
    { title: 'Condition', dataIndex: 'conditionType', key: 'conditionType', width: 140, render: (v: string) => <TypeBadge type={v} /> },
    { title: 'Target', dataIndex: 'conditionValue', key: 'conditionValue', width: 70, render: (v: number) => <span style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}>{v}</span> },
    { title: 'XP', dataIndex: 'rewardXp', key: 'rewardXp', width: 70, render: (v: number) => <span style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}>{v ?? '—'}</span> },
  ];

  const tabs: { key: TabKey; label: string }[] = [
    { key: 'quests', label: 'Daily Quests' },
    { key: 'achievements', label: 'Achievements' },
  ];

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
          onClick={() => setIsModalOpen(true)}
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

      {/* Create Modal */}
      <Modal
        title={`Create ${activeTab === 'quests' ? 'Quest' : 'Achievement'}`}
        open={isModalOpen}
        onCancel={() => { setIsModalOpen(false); form.resetFields(); }}
        onOk={() => form.submit()}
        okText="Create"
        width={400}
      >
        <Form form={form} layout="vertical" onFinish={handleCreate} style={{ marginTop: 16 }}>
          {activeTab === 'quests' ? (
            <>
              <Form.Item name="title" label="Quest Title" rules={[{ required: true }]}><Input /></Form.Item>
              <Form.Item name="questType" label="Type" rules={[{ required: true }]}>
                <Select>
                  <Select.Option value="LearnWords">Learn Words</Select.Option>
                  <Select.Option value="ReviewWords">Review Words</Select.Option>
                  <Select.Option value="WinPvP">Win PvP</Select.Option>
                  <Select.Option value="PerfectScore">Perfect Score</Select.Option>
                </Select>
              </Form.Item>
              <Form.Item name="targetCount" label="Target Count" rules={[{ required: true }]}><InputNumber style={{ width: '100%' }} /></Form.Item>
              <Form.Item name="rewardXp" label="XP Reward"><InputNumber style={{ width: '100%' }} /></Form.Item>
              <Form.Item name="rewardAp" label="AP Reward"><InputNumber style={{ width: '100%' }} /></Form.Item>
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
                </Select>
              </Form.Item>
              <Form.Item name="conditionValue" label="Target Value" rules={[{ required: true }]}><InputNumber style={{ width: '100%' }} /></Form.Item>
              <Form.Item name="rewardXp" label="XP Reward"><InputNumber style={{ width: '100%' }} /></Form.Item>
            </>
          )}
        </Form>
      </Modal>
    </div>
  );
}
