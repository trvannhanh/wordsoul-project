'use client';

import React, { useEffect, useState } from 'react';
import { Row, Col, Card, Button, Table, Tag, Spin } from 'antd';
import { useAuth } from '@/contexts/AuthContext';
import {
  TeamOutlined,
  BookOutlined,
  RiseOutlined,
  UserAddOutlined,
  PlusOutlined,
  BarChartOutlined,
  TrophyOutlined,
} from '@ant-design/icons';
import { useRouter } from 'next/navigation';
import {
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
  Tooltip,
} from 'recharts';
import { authApi, endpoints } from '@/services/api';

// ── Types ────────────────────────────────────────────────────────────────────
interface TopUser {
  userId: number;
  userName: string;
  totalXP: number;
  totalAP: number;
}

interface DashboardStats {
  totalUsers: number;
  activeUsersToday: number;
  totalVocabularySets: number;
  totalLearningSessions: number;
  newUsersThisWeek: number;
  topXpUsers: TopUser[];
}

// ── Static chart data (kept as-is — requires event tracking to make dynamic) ─
const memoryStateData = [
  { name: 'New',        value: 400 },
  { name: 'Learning',   value: 300 },
  { name: 'Review',     value: 800 },
  { name: 'Mastered',   value: 600 },
  { name: 'Relearning', value: 150 },
];

const PIE_COLORS = ['#6366F1', '#818CF8', '#A5B4FC', '#C7D2FE', '#E0E7FF'];

// ── Shared styles ─────────────────────────────────────────────────────────────
const cardStyle = {
  background: 'var(--bg-surface)',
  border: '1px solid var(--border)',
  borderRadius: 8,
};

// ── Custom tooltip for recharts ───────────────────────────────────────────────
function CustomTooltip({ active, payload, label }: { active?: boolean; payload?: Array<{ dataKey: string; name: string; value: number; color: string }>; label?: string }) {
  if (!active || !payload?.length) return null;
  return (
    <div
      style={{
        background: 'var(--bg-surface)',
        border: '1px solid var(--border)',
        borderRadius: 6,
        padding: '8px 12px',
        fontSize: 12,
        color: 'var(--text-primary)',
        boxShadow: 'var(--shadow-md)',
      }}
    >
      <div style={{ color: 'var(--text-muted)', marginBottom: 4 }}>{label}</div>
      {payload.map((p) => (
        <div key={p.dataKey} style={{ color: p.color, fontWeight: 600 }}>
          {p.name}: {p.value.toLocaleString()}
        </div>
      ))}
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────
export default function DashboardPage() {
  const { user } = useAuth();
  const router = useRouter();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    authApi.get<DashboardStats>(endpoints.dashboard)
      .then(res => setStats(res.data))
      .catch(() => {/* silently fail — stats stay null */})
      .finally(() => setLoading(false));
  }, []);

  const kpiCards = [
    {
      icon: <TeamOutlined style={{ color: 'var(--accent)' }} />,
      label: 'Total Registered Users',
      value: stats ? stats.totalUsers.toLocaleString() : '—',
      meta: stats ? `+${stats.newUsersThisWeek} this week` : 'Loading...',
      metaColor: 'var(--success)',
    },
    {
      icon: <RiseOutlined style={{ color: 'var(--accent)' }} />,
      label: 'Active Users Today',
      value: stats ? stats.activeUsersToday.toLocaleString() : '—',
      meta: 'Unique users with sessions',
      metaColor: 'var(--text-muted)',
    },
    {
      icon: <BookOutlined style={{ color: 'var(--accent)' }} />,
      label: 'Vocabulary Sets',
      value: stats ? stats.totalVocabularySets.toLocaleString() : '—',
      meta: 'Published on platform',
      metaColor: 'var(--text-muted)',
    },
    {
      icon: <BarChartOutlined style={{ color: 'var(--accent)' }} />,
      label: 'Total Learning Sessions',
      value: stats ? stats.totalLearningSessions.toLocaleString() : '—',
      meta: 'All time',
      metaColor: 'var(--text-muted)',
    },
  ];

  const topUsersColumns = [
    {
      title: 'Rank',
      render: (_: unknown, __: TopUser, idx: number) => (
        <span style={{ fontWeight: 600, color: idx === 0 ? '#F59E0B' : idx === 1 ? '#9CA3AF' : idx === 2 ? '#B45309' : 'var(--text-muted)' }}>
          #{idx + 1}
        </span>
      ),
      width: 52,
    },
    {
      title: 'User',
      dataIndex: 'userName',
      render: (name: string) => <span style={{ fontWeight: 500 }}>{name}</span>,
    },
    {
      title: 'XP',
      dataIndex: 'totalXP',
      render: (v: number) => <Tag color="blue">{v.toLocaleString()}</Tag>,
    },
    {
      title: 'AP',
      dataIndex: 'totalAP',
      render: (v: number) => <Tag color="gold">{v.toLocaleString()}</Tag>,
    },
  ];

  return (
    <div>
      {/* Page header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Overview</h1>
          <p className="page-subtitle">
            Welcome back, {user?.name}. Here&apos;s what&apos;s happening on the platform today.
          </p>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <Button
            size="small"
            icon={<UserAddOutlined />}
            onClick={() => router.push('/users')}
          >
            Manage Users
          </Button>
          <Button
            type="primary"
            size="small"
            icon={<PlusOutlined />}
            onClick={() => router.push('/vocabularies')}
          >
            New Vocabulary Set
          </Button>
        </div>
      </div>

      {/* ── KPI Strip ──────────────────────────────────────────────────────── */}
      <Row gutter={[12, 12]} style={{ marginBottom: 16 }}>
        {kpiCards.map(card => (
          <Col xs={24} sm={12} lg={6} key={card.label}>
            <div className="stat-card">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 10 }}>
                <span style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  {card.label}
                </span>
                <span style={{ fontSize: 16 }}>{card.icon}</span>
              </div>
              {loading ? (
                <Spin size="small" />
              ) : (
                <>
                  <div className="stat-value">{card.value}</div>
                  <div className="stat-meta" style={{ color: card.metaColor }}>{card.meta}</div>
                </>
              )}
            </div>
          </Col>
        ))}
      </Row>

      {/* ── Charts + Leaderboard ────────────────────────────────────────────── */}
      <Row gutter={[12, 12]}>
        {/* SM-2 Memory States Pie */}
        <Col xs={24} lg={14}>
          <Card
            styles={{ body: { padding: '16px 20px' }, header: { borderBottom: '1px solid var(--border)', padding: '12px 20px' } }}
            style={cardStyle}
            title={<span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>Global Memory States (SM-2)</span>}
            variant="borderless"
          >
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie
                  data={memoryStateData}
                  cx="45%"
                  cy="50%"
                  innerRadius={50}
                  outerRadius={80}
                  paddingAngle={2}
                  dataKey="value"
                >
                  {memoryStateData.map((_, idx) => (
                    <Cell key={idx} fill={PIE_COLORS[idx % PIE_COLORS.length]} />
                  ))}
                </Pie>
                <Legend
                  iconType="circle"
                  iconSize={8}
                  formatter={val => (
                    <span style={{ fontSize: 11, color: 'var(--text-secondary)' }}>{val}</span>
                  )}
                />
                <Tooltip content={<CustomTooltip />} />
              </PieChart>
            </ResponsiveContainer>
          </Card>
        </Col>

        {/* Top XP Leaderboard */}
        <Col xs={24} lg={10}>
          <Card
            styles={{ body: { padding: 0 }, header: { borderBottom: '1px solid var(--border)', padding: '12px 20px' } }}
            style={cardStyle}
            title={
              <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)', display: 'flex', alignItems: 'center', gap: 6 }}>
                <TrophyOutlined style={{ color: '#F59E0B' }} /> Top XP Players
              </span>
            }
            variant="borderless"
          >
            <Table
              dataSource={stats?.topXpUsers ?? []}
              columns={topUsersColumns}
              rowKey="userId"
              pagination={false}
              loading={loading}
              size="small"
              style={{ background: 'transparent' }}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
}
