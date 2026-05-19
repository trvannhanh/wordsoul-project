'use client';

import React, { useState, useEffect } from 'react';
import {
  Drawer, Tabs, Avatar, Tag, Spin, Empty,
  Descriptions, Timeline, Badge, Statistic, Row, Col,
} from 'antd';
import {
  UserOutlined, TrophyOutlined, ThunderboltOutlined,
  StarOutlined, HistoryOutlined, HeartOutlined,
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
  const [detail, setDetail] = useState<UserDetail | null>(null);
  const [activities, setActivities] = useState<ActivityLog[]>([]);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [loadingActivity, setLoadingActivity] = useState(false);
  const [activeTab, setActiveTab] = useState('profile');

  useEffect(() => {
    if (!open || !userId) return;
    setDetail(null);
    setActivities([]);
    setActiveTab('profile');

    setLoadingDetail(true);
    authApi
      .get(endpoints.userDetail(userId))
      .then((r) => setDetail(r.data))
      .finally(() => setLoadingDetail(false));
  }, [open, userId]);

  const fetchActivities = () => {
    if (!userId || activities.length > 0) return;
    setLoadingActivity(true);
    authApi
      .get(`${endpoints.userActivities(userId)}?pageNumber=1&pageSize=30`)
      .then((r) => setActivities(r.data))
      .finally(() => setLoadingActivity(false));
  };

  const pvpTotal = (detail?.pvpWins ?? 0) + (detail?.pvpLosses ?? 0);
  const winRate = pvpTotal > 0 ? Math.round(((detail?.pvpWins ?? 0) / pvpTotal) * 100) : 0;

  return (
    <Drawer
      title={null}
      open={open}
      onClose={onClose}
      width={440}
      styles={{
        body: { padding: 0, background: 'var(--bg-base)' },
        header: { display: 'none' },
        wrapper: { boxShadow: '-4px 0 24px rgba(0,0,0,0.08)' },
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
                            valueStyle={{ fontSize: 16, fontWeight: 700 }}
                          />
                        </Col>
                        <Col span={8}>
                          <Statistic
                            title={<span style={{ fontSize: 11 }}>W / L</span>}
                            value={`${detail.pvpWins} / ${detail.pvpLosses}`}
                            valueStyle={{ fontSize: 14, fontWeight: 700 }}
                          />
                        </Col>
                        <Col span={8}>
                          <Statistic
                            title={<span style={{ fontSize: 11 }}>Win Rate</span>}
                            value={winRate}
                            suffix="%"
                            valueStyle={{ fontSize: 16, fontWeight: 700, color: winRate >= 50 ? 'var(--success)' : 'var(--danger)' }}
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
                          children: (
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
            ]}
          />
        </>
      ) : null}
    </Drawer>
  );
}
