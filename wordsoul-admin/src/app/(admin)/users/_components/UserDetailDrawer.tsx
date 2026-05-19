'use client';

import React, { useState, useEffect } from 'react';
import {
  App, Button, Drawer, Form, Input, InputNumber,
  Modal, Tabs, Avatar, Spin, Empty,
  Descriptions, Timeline, Badge, Statistic, Row, Col, Progress, Table, Tag,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  UserOutlined, TrophyOutlined, ThunderboltOutlined,
  StarOutlined, HistoryOutlined, HeartOutlined, EditOutlined, BookOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import dayjs from 'dayjs';

interface UserDetail {
  id: number;
  username: string;
  email: string;
  role: string;
  createdAt: string;
  isActive: boolean;
  level: number;
  totalXP: number;
  totalAP: number;
  hintBalance: number;
  streakDays: number;
  petCount: number;
  pvpRating: number;
  pvpWins: number;
  pvpLosses: number;
  avatarUrl?: string;
}

interface ActivityLog {
  id: number;
  action: string;
  details: string;
  timestamp: string;
}

interface StruggleWord {
  word: string;
  meaning?: string;
  wrongCount: number;
  retentionScore: number;
}

interface LearningProgress {
  newCount: number;
  learningCount: number;
  reviewCount: number;
  masteredCount: number;
  totalVocabularies: number;
  dueForReviewCount: number;
  nextReviewTime?: string;
  totalCorrect: number;
  totalWrong: number;
  accuracyRate: number;
  averageRetentionScore: number;
  totalSessions: number;
  completedSessions: number;
  struggleWords: StruggleWord[];
}

const ROLE_COLOR: Record<string, string> = {
  SuperAdmin: 'var(--accent)',
  Admin: '#0369A1',
  User: 'var(--text-muted)',
};

export default function UserDetailDrawer({
  userId,
  open,
  onClose,
}: {
  userId: number | null;
  open: boolean;
  onClose: () => void;
}) {
  const { message } = App.useApp();
  const [adjustForm] = Form.useForm();
  const [detail, setDetail] = useState<UserDetail | null>(null);
  const [activities, setActivities] = useState<ActivityLog[]>([]);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [loadingActivity, setLoadingActivity] = useState(false);
  const [activeTab, setActiveTab] = useState('profile');
  const [adjustOpen, setAdjustOpen] = useState(false);
  const [adjusting, setAdjusting] = useState(false);
  const [learningProgress, setLearningProgress] = useState<LearningProgress | null>(null);
  const [loadingProgress, setLoadingProgress] = useState(false);

  useEffect(() => {
    if (!open || !userId) return;
    setDetail(null);
    setActivities([]);
    setLearningProgress(null);
    setActiveTab('profile');

    setLoadingDetail(true);
    authApi
      .get(endpoints.userDetail(userId))
      .then((r) => setDetail(r.data))
      .finally(() => setLoadingDetail(false));
  }, [open, userId]);

  const handleAdjust = async () => {
    if (!detail) return;
    try {
      const values = await adjustForm.validateFields();
      setAdjusting(true);
      await authApi.patch(endpoints.userBalance(detail.id), values);
      const updated = await authApi.get(endpoints.userDetail(detail.id));
      setDetail(updated.data);
      setAdjustOpen(false);
      adjustForm.resetFields();
      message.success('Balance adjusted successfully');
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'errorFields' in err) return;
      message.error('Failed to adjust balance');
    } finally {
      setAdjusting(false);
    }
  };

  const fetchActivities = () => {
    if (!userId || activities.length > 0) return;
    setLoadingActivity(true);
    authApi
      .get(`${endpoints.userActivities(userId)}?pageNumber=1&pageSize=30`)
      .then((r) => setActivities(r.data))
      .finally(() => setLoadingActivity(false));
  };

  const fetchLearningProgress = () => {
    if (!userId || learningProgress) return;
    setLoadingProgress(true);
    authApi
      .get(endpoints.userLearningProgress(userId))
      .then((r) => setLearningProgress(r.data))
      .finally(() => setLoadingProgress(false));
  };

  const pvpTotal = (detail?.pvpWins ?? 0) + (detail?.pvpLosses ?? 0);
  const winRate = pvpTotal > 0 ? Math.round(((detail?.pvpWins ?? 0) / pvpTotal) * 100) : 0;

  return (
    <Drawer
      title={null}
      open={open}
      onClose={onClose}
      styles={{
        body: { padding: 0, background: 'var(--bg-base)' },
        header: { display: 'none' },
        wrapper: { width: 440, boxShadow: '-4px 0 24px rgba(0,0,0,0.08)' },
      }}
    >
      {loadingDetail ? (
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: 300 }}>
          <Spin />
        </div>
      ) : detail ? (
        <>
          {/* ── Header ── */}
          <div
            style={{
              padding: '24px 24px 16px',
              background: 'var(--bg-surface)',
              borderBottom: '1px solid var(--border)',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
              <Avatar
                size={52}
                src={detail.avatarUrl}
                style={{
                  background: 'var(--accent-subtle)',
                  color: 'var(--accent)',
                  fontSize: 20,
                  fontWeight: 700,
                  flexShrink: 0,
                }}
              >
                {detail.username.charAt(0).toUpperCase()}
              </Avatar>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontSize: 15, fontWeight: 700, color: 'var(--text-primary)' }}>
                  {detail.username}
                </div>
                <div style={{ fontSize: 12, color: 'var(--text-muted)', marginTop: 1 }}>{detail.email}</div>
                <div style={{ display: 'flex', gap: 6, marginTop: 6, flexWrap: 'wrap' }}>
                  <span
                    style={{
                      fontSize: 11,
                      fontWeight: 600,
                      padding: '1px 7px',
                      borderRadius: 4,
                      color: ROLE_COLOR[detail.role] ?? 'var(--text-muted)',
                      background: 'var(--bg-muted)',
                      border: '1px solid var(--border)',
                    }}
                  >
                    {detail.role}
                  </span>
                  <Badge
                    status={detail.isActive ? 'success' : 'error'}
                    text={
                      <span style={{ fontSize: 11, color: detail.isActive ? 'var(--success)' : 'var(--danger)' }}>
                        {detail.isActive ? 'Active' : 'Banned'}
                      </span>
                    }
                  />
                </div>
              </div>
            </div>
          </div>

          {/* ── Tabs ── */}
          <Tabs
            activeKey={activeTab}
            onChange={(k) => {
              setActiveTab(k);
              if (k === 'activity') fetchActivities();
              if (k === 'learning') fetchLearningProgress();
            }}
            size="small"
            style={{ padding: '0 16px' }}
            tabBarStyle={{ borderBottom: '1px solid var(--border)', marginBottom: 0 }}
            items={[
              {
                key: 'profile',
                label: (
                  <span style={{ fontSize: 12 }}>
                    <UserOutlined style={{ marginRight: 4 }} />
                    Profile
                  </span>
                ),
                children: (
                  <div style={{ padding: '16px 8px' }}>
                    {/* Stats grid */}
                    <Row gutter={[12, 12]} style={{ marginBottom: 16 }}>
                      {[
                        { icon: <StarOutlined />, label: 'Level', value: detail.level, color: '#f59e0b' },
                        { icon: <ThunderboltOutlined />, label: 'XP', value: detail.totalXP.toLocaleString(), color: 'var(--accent)' },
                        { icon: <TrophyOutlined />, label: 'AP', value: detail.totalAP.toLocaleString(), color: '#0369a1' },
                        { icon: <HeartOutlined />, label: 'Hints', value: detail.hintBalance, color: '#e11d48' },
                      ].map(({ icon, label, value, color }) => (
                        <Col span={12} key={label}>
                          <div
                            style={{
                              background: 'var(--bg-surface)',
                              border: '1px solid var(--border)',
                              borderRadius: 8,
                              padding: '10px 12px',
                            }}
                          >
                            <div style={{ color, fontSize: 14, marginBottom: 4 }}>{icon}</div>
                            <div style={{ fontSize: 16, fontWeight: 700, color: 'var(--text-primary)' }}>{value}</div>
                            <div style={{ fontSize: 11, color: 'var(--text-muted)' }}>{label}</div>
                          </div>
                        </Col>
                      ))}
                    </Row>

                    {/* Adjust Balance button */}
                    <div style={{ textAlign: 'right', marginBottom: 12 }}>
                      <Button
                        size="small"
                        icon={<EditOutlined />}
                        onClick={() => { adjustForm.resetFields(); setAdjustOpen(true); }}
                      >
                        Adjust Balance
                      </Button>
                    </div>

                    {/* Details */}
                    <Descriptions
                      column={1}
                      size="small"
                      styles={{ label: { fontSize: 12, color: 'var(--text-muted)', width: 120 }, content: { fontSize: 12 } }}
                    >
                      <Descriptions.Item label="Joined">
                        {dayjs(detail.createdAt).format('MMM D, YYYY')}
                      </Descriptions.Item>
                      <Descriptions.Item label="Streak">
                        {detail.streakDays} day{detail.streakDays !== 1 ? 's' : ''}
                      </Descriptions.Item>
                      <Descriptions.Item label="Pets">
                        {detail.petCount}
                      </Descriptions.Item>
                    </Descriptions>

                    {/* PvP section */}
                    <div
                      style={{
                        marginTop: 16,
                        background: 'var(--bg-surface)',
                        border: '1px solid var(--border)',
                        borderRadius: 8,
                        padding: '12px 16px',
                      }}
                    >
                      <div
                        style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: 10 }}
                      >
                        PvP Stats
                      </div>
                      <Row gutter={16}>
                        <Col span={8}>
                          <Statistic
                            title={<span style={{ fontSize: 11 }}>Rating</span>}
                            value={detail.pvpRating}
                            styles={{ content: { fontSize: 16, fontWeight: 700 } }}
                          />
                        </Col>
                        <Col span={8}>
                          <Statistic
                            title={<span style={{ fontSize: 11 }}>W / L</span>}
                            value={`${detail.pvpWins} / ${detail.pvpLosses}`}
                            styles={{ content: { fontSize: 14, fontWeight: 700 } }}
                          />
                        </Col>
                        <Col span={8}>
                          <Statistic
                            title={<span style={{ fontSize: 11 }}>Win Rate</span>}
                            value={winRate}
                            suffix="%"
                            styles={{ content: { fontSize: 16, fontWeight: 700, color: winRate >= 50 ? 'var(--success)' : 'var(--danger)' } }}
                          />
                        </Col>
                      </Row>
                    </div>
                  </div>
                ),
              },
              {
                key: 'activity',
                label: (
                  <span style={{ fontSize: 12 }}>
                    <HistoryOutlined style={{ marginRight: 4 }} />
                    Activity
                  </span>
                ),
                children: (
                  <div style={{ padding: '16px 8px' }}>
                    {loadingActivity ? (
                      <div style={{ textAlign: 'center', padding: 32 }}>
                        <Spin size="small" />
                      </div>
                    ) : activities.length === 0 ? (
                      <Empty description="No activity found" image={Empty.PRESENTED_IMAGE_SIMPLE} />
                    ) : (
                      <Timeline
                        style={{ paddingTop: 8 }}
                        items={activities.map((a) => ({
                          key: a.id,
                          content: (
                            <div>
                              <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-primary)' }}>{a.action}</div>
                              {a.details && (
                                <div style={{ fontSize: 11, color: 'var(--text-muted)', marginTop: 1 }}>{a.details}</div>
                              )}
                              <div style={{ fontSize: 10, color: 'var(--text-muted)', marginTop: 2 }}>
                                {dayjs(a.timestamp).format('MMM D, YYYY HH:mm')}
                              </div>
                            </div>
                          ),
                        }))}
                      />
                    )}
                  </div>
                ),
              },
              {
                key: 'learning',
                label: (
                  <span style={{ fontSize: 12 }}>
                    <BookOutlined style={{ marginRight: 4 }} />
                    Learning
                  </span>
                ),
                children: (
                  <div style={{ padding: '16px 8px' }}>
                    {loadingProgress ? (
                      <div style={{ textAlign: 'center', padding: 32 }}><Spin size="small" /></div>
                    ) : !learningProgress ? (
                      <Empty description="No learning data" image={Empty.PRESENTED_IMAGE_SIMPLE} />
                    ) : (
                      <>
                        {/* Memory State bar */}
                        <div
                          style={{
                            background: 'var(--bg-surface)',
                            border: '1px solid var(--border)',
                            borderRadius: 8,
                            padding: '12px 16px',
                            marginBottom: 12,
                          }}
                        >
                          <div style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: 10 }}>
                            Memory State — {learningProgress.totalVocabularies} words
                          </div>
                          {[
                            { label: 'New',      count: learningProgress.newCount,      color: '#6b7280' },
                            { label: 'Learning', count: learningProgress.learningCount, color: '#3b82f6' },
                            { label: 'Review',   count: learningProgress.reviewCount,   color: '#f59e0b' },
                            { label: 'Mastered', count: learningProgress.masteredCount, color: '#10b981' },
                          ].map(({ label, count, color }) => (
                            <div key={label} style={{ marginBottom: 6 }}>
                              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 11, marginBottom: 2 }}>
                                <span style={{ color }}>{label}</span>
                                <span style={{ color: 'var(--text-muted)' }}>{count}</span>
                              </div>
                              <Progress
                                percent={learningProgress.totalVocabularies > 0
                                  ? Math.round(count / learningProgress.totalVocabularies * 100)
                                  : 0}
                                strokeColor={color}
                                showInfo={false}
                                size="small"
                              />
                            </div>
                          ))}
                        </div>

                        {/* Stats row */}
                        <Row gutter={[10, 10]} style={{ marginBottom: 12 }}>
                          {[
                            { label: 'Due for Review', value: learningProgress.dueForReviewCount, color: '#f59e0b' },
                            { label: 'Accuracy',       value: `${learningProgress.accuracyRate}%`, color: '#10b981' },
                            { label: 'Avg Retention',  value: `${learningProgress.averageRetentionScore}%`, color: '#3b82f6' },
                          ].map(({ label, value, color }) => (
                            <Col span={8} key={label}>
                              <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, padding: '8px 10px', textAlign: 'center' }}>
                                <div style={{ fontSize: 15, fontWeight: 700, color }}>{value}</div>
                                <div style={{ fontSize: 10, color: 'var(--text-muted)', marginTop: 2 }}>{label}</div>
                              </div>
                            </Col>
                          ))}
                        </Row>

                        {/* Session summary */}
                        <div style={{ fontSize: 11, color: 'var(--text-muted)', marginBottom: 8 }}>
                          Sessions (last 30 days): <strong style={{ color: 'var(--text-primary)' }}>{learningProgress.completedSessions}</strong> completed / {learningProgress.totalSessions} total
                        </div>

                        {/* Struggle words */}
                        {learningProgress.struggleWords.length > 0 && (
                          <>
                            <div style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: 6 }}>
                              Top Struggle Words
                            </div>
                            <Table<StruggleWord>
                              dataSource={learningProgress.struggleWords}
                              rowKey="word"
                              size="small"
                              pagination={false}
                              columns={[
                                { title: 'Word', dataIndex: 'word', key: 'word', render: (w) => <strong>{w}</strong> },
                                { title: 'Meaning', dataIndex: 'meaning', key: 'meaning', ellipsis: true, render: (m) => m ?? '—' },
                                { title: '✗', dataIndex: 'wrongCount', key: 'wrongCount', width: 44, align: 'center', render: (n) => <Tag color="red" style={{ fontSize: 10, margin: 0 }}>{n}</Tag> },
                              ] as ColumnsType<StruggleWord>}
                            />
                          </>
                        )}
                      </>
                    )}
                  </div>
                ),
              },
            ]}
          />
        </>
      ) : null}

      {/* Adjust Balance Modal */}
      <Modal
        title="Adjust User Balance"
        open={adjustOpen}
        onOk={handleAdjust}
        onCancel={() => { setAdjustOpen(false); adjustForm.resetFields(); }}
        confirmLoading={adjusting}
        okText="Apply"
      >
        <Form form={adjustForm} layout="vertical" style={{ marginTop: 8 }}>
          <Row gutter={12}>
            <Col span={8}>
              <Form.Item name="xpDelta" label="XP Delta" initialValue={0}>
                <InputNumber style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="apDelta" label="AP Delta" initialValue={0}>
                <InputNumber style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="hintDelta" label="Hint Delta" initialValue={0}>
                <InputNumber style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item
            name="reason"
            label="Reason"
            rules={[{ required: true, message: 'Reason is required' }, { max: 300 }]}
          >
            <Input.TextArea rows={2} placeholder="e.g. Compensation for bug" maxLength={300} showCount />
          </Form.Item>
          {detail && (
            <div style={{ fontSize: 12, color: 'var(--text-muted)' }}>
              Preview — XP: <strong>{detail.totalXP}</strong> → <strong>{Math.max(0, detail.totalXP + (adjustForm.getFieldValue('xpDelta') || 0))}</strong>
              {' · '}AP: <strong>{detail.totalAP}</strong> → <strong>{Math.max(0, detail.totalAP + (adjustForm.getFieldValue('apDelta') || 0))}</strong>
              {' · '}Hints: <strong>{detail.hintBalance}</strong> → <strong>{Math.max(0, detail.hintBalance + (adjustForm.getFieldValue('hintDelta') || 0))}</strong>
            </div>
          )}
        </Form>
      </Modal>
    </Drawer>
  );
}
