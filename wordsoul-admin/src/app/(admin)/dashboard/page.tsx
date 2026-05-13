'use client';

import React from 'react';
import { Row, Col, Card, Button } from 'antd';
import { useAuth } from '@/contexts/AuthContext';
import {
  TeamOutlined,
  BookOutlined,
  RiseOutlined,
  UserAddOutlined,
  PlusOutlined,
  BarChartOutlined,
} from '@ant-design/icons';
import { useRouter } from 'next/navigation';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts';

// ── Mock Data ─────────────────────────────────────────────────────────────────
const dauData = [
  { day: 'Mon', dau: 1200 },
  { day: 'Tue', dau: 1310 },
  { day: 'Wed', dau: 1090 },
  { day: 'Thu', dau: 1420 },
  { day: 'Fri', dau: 1530 },
  { day: 'Sat', dau: 1780 },
  { day: 'Sun', dau: 1940 },
];

const memoryStateData = [
  { name: 'New',       value: 400 },
  { name: 'Learning',  value: 300 },
  { name: 'Review',    value: 800 },
  { name: 'Mastered',  value: 600 },
  { name: 'Relearning',value: 150 },
];

// Desaturated palette for pie chart
const PIE_COLORS = ['#6366F1', '#818CF8', '#A5B4FC', '#C7D2FE', '#E0E7FF'];

const recentActivity = [
  { time: '10:14', action: 'User banned', detail: 'user#4821 flagged for abuse' },
  { time: '09:52', action: 'Vocab set published', detail: '"TOEIC Business B2" · 45 words' },
  { time: '09:31', action: 'AI generation used', detail: 'Set "Travel Phrases" created via Gemini' },
  { time: '08:47', action: 'Role updated', detail: 'user#3310 → Admin' },
  { time: '08:20', action: 'System config saved', detail: 'SRS EASE_FACTOR updated to 2.6' },
];

// ── Shared styles ─────────────────────────────────────────────────────────────
const sectionTitle = {
  fontSize: 13,
  fontWeight: 600,
  color: 'var(--text-secondary)',
  marginBottom: 12,
  textTransform: 'uppercase' as const,
  letterSpacing: '0.05em',
};

const cardStyle = {
  background: 'var(--bg-surface)',
  border: '1px solid var(--border)',
  borderRadius: 8,
};

// ── Custom tooltip for recharts ───────────────────────────────────────────────
function CustomTooltip({ active, payload, label }: any) {
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
      {payload.map((p: any) => (
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
        {[
          {
            icon: <TeamOutlined style={{ color: 'var(--accent)' }} />,
            label: 'Daily Active Users',
            value: '1,940',
            meta: '↑ 8.4% vs last week',
            metaColor: 'var(--success)',
          },
          {
            icon: <BookOutlined style={{ color: 'var(--accent)' }} />,
            label: 'Vocabulary Sets Published',
            value: '284',
            meta: '12 added this week',
            metaColor: 'var(--text-muted)',
          },
          {
            icon: <RiseOutlined style={{ color: 'var(--accent)' }} />,
            label: 'Avg. Retention Rate (7d)',
            value: '68.5%',
            meta: 'SM-2 algorithm',
            metaColor: 'var(--text-muted)',
          },
          {
            icon: <BarChartOutlined style={{ color: 'var(--accent)' }} />,
            label: 'Total Registered Users',
            value: '5,841',
            meta: '↑ 3.2% this month',
            metaColor: 'var(--success)',
          },
        ].map(card => (
          <Col xs={24} sm={12} lg={6} key={card.label}>
            <div className="stat-card">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 10 }}>
                <span style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  {card.label}
                </span>
                <span style={{ fontSize: 16 }}>{card.icon}</span>
              </div>
              <div className="stat-value">{card.value}</div>
              <div className="stat-meta" style={{ color: card.metaColor }}>
                {card.meta}
              </div>
            </div>
          </Col>
        ))}
      </Row>

      {/* ── Charts + Activity ───────────────────────────────────────────────── */}
      <Row gutter={[12, 12]}>
        {/* DAU Line Chart */}
        <Col xs={24} lg={14}>
          <Card
            styles={{ body: { padding: '16px 20px' }, header: { borderBottom: '1px solid var(--border)', padding: '12px 20px' } }}
            style={cardStyle}
            title={<span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>Daily Active Users — Last 7 Days</span>}
            variant="borderless"
          >
            <ResponsiveContainer width="100%" height={220}>
              <LineChart data={dauData} margin={{ top: 4, right: 4, bottom: 0, left: -20 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
                <XAxis dataKey="day" tick={{ fontSize: 11, fill: 'var(--text-muted)' }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fontSize: 11, fill: 'var(--text-muted)' }} axisLine={false} tickLine={false} />
                <Tooltip content={<CustomTooltip />} />
                <Line
                  type="monotone"
                  dataKey="dau"
                  name="DAU"
                  stroke="var(--accent)"
                  strokeWidth={2}
                  dot={{ r: 3, fill: 'var(--accent)', strokeWidth: 0 }}
                  activeDot={{ r: 5 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </Card>
        </Col>

        {/* SM-2 Memory States Pie */}
        <Col xs={24} lg={10}>
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

        {/* Recent Activity */}
        <Col xs={24}>
          <Card
            styles={{ body: { padding: 0 }, header: { borderBottom: '1px solid var(--border)', padding: '12px 20px' } }}
            style={cardStyle}
            title={<span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>Recent Activity</span>}
            variant="borderless"
          >
            {recentActivity.map((item, idx) => (
              <div
                key={idx}
                style={{
                  display: 'flex',
                  alignItems: 'baseline',
                  gap: 12,
                  padding: '10px 20px',
                  borderBottom: idx < recentActivity.length - 1 ? '1px solid var(--border)' : 'none',
                }}
              >
                <span style={{ fontSize: 11, color: 'var(--text-muted)', minWidth: 36, fontVariantNumeric: 'tabular-nums' }}>
                  {item.time}
                </span>
                <span style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-primary)', minWidth: 160 }}>
                  {item.action}
                </span>
                <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
                  {item.detail}
                </span>
              </div>
            ))}
          </Card>
        </Col>
      </Row>
    </div>
  );
}
