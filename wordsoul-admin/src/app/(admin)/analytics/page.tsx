'use client';

import { useCallback, useEffect, useState } from 'react';
import {
  Button, Card, Col, Row, Select, Spin, Statistic, Table, Typography,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  BarChartOutlined, CheckCircleOutlined, ClockCircleOutlined,
  QuestionCircleOutlined, ReloadOutlined, ThunderboltOutlined, UserOutlined,
} from '@ant-design/icons';
import {
  Area, AreaChart, CartesianGrid, Legend,
  Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis,
} from 'recharts';
import { authApi, endpoints } from '@/services/api';

const { Text } = Typography;

// ── Types ────────────────────────────────────────────────────────────────────
interface DailyStat {
  date: string;
  learningSessions: number;
  reviewSessions: number;
  correctAnswers: number;
  totalAnswers: number;
}

interface ActiveUser {
  userId: number;
  userName: string;
  sessionCount: number;
  correctAnswers: number;
}

interface SessionAnalytics {
  totalSessions: number;
  learningSessions: number;
  reviewSessions: number;
  completedSessions: number;
  completionRate: number;
  totalAnswers: number;
  correctAnswers: number;
  overallAccuracy: number;
  avgResponseTimeSeconds: number;
  totalHintsUsed: number;
  dailyStats: DailyStat[];
  topActiveUsers: ActiveUser[];
}

// ── Columns ──────────────────────────────────────────────────────────────────
const userColumns: ColumnsType<ActiveUser> = [
  {
    title: 'Rank',
    key: 'rank',
    width: 52,
    align: 'center',
    render: (_, __, i) => {
      const medals = ['🥇', '🥈', '🥉'];
      return medals[i]
        ? <span style={{ fontSize: 16 }}>{medals[i]}</span>
        : <Text type="secondary">#{i + 1}</Text>;
    },
  },
  {
    title: 'User',
    dataIndex: 'userName',
    key: 'userName',
    render: (name) => (
      <span style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
        <UserOutlined style={{ color: '#6366f1' }} />
        <Text strong>{name}</Text>
      </span>
    ),
  },
  {
    title: 'Sessions',
    dataIndex: 'sessionCount',
    key: 'sessionCount',
    width: 90,
    align: 'center',
    render: (n) => <Text strong style={{ color: '#6366f1' }}>{n}</Text>,
  },
  {
    title: 'Correct Answers',
    dataIndex: 'correctAnswers',
    key: 'correctAnswers',
    width: 130,
    align: 'center',
    render: (n) => <Text style={{ color: '#10b981' }}>{n}</Text>,
  },
];

// ── Day options ───────────────────────────────────────────────────────────────
const DAY_OPTIONS = [
  { label: 'Last 7 days',  value: 7 },
  { label: 'Last 14 days', value: 14 },
  { label: 'Last 30 days', value: 30 },
  { label: 'Last 90 days', value: 90 },
];

// ── Page ─────────────────────────────────────────────────────────────────────
export default function SessionAnalyticsPage() {
  const [data, setData]       = useState<SessionAnalytics | null>(null);
  const [loading, setLoading] = useState(false);
  const [days, setDays]       = useState(30);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await authApi.get(endpoints.sessionAnalytics, { params: { days } });
      setData(res.data);
    } finally {
      setLoading(false);
    }
  }, [days]);

  useEffect(() => { load(); }, [load]);

  // Enrich daily stats with accuracy %
  const chartData = (data?.dailyStats ?? []).map((d) => ({
    ...d,
    accuracy: d.totalAnswers > 0 ? Math.round(d.correctAnswers / d.totalAnswers * 100) : 0,
    totalSessions: d.learningSessions + d.reviewSessions,
  }));

  const controls = (
    <div style={{ display: 'flex', gap: 8 }}>
      <Select
        value={days}
        onChange={setDays}
        options={DAY_OPTIONS}
        style={{ width: 130 }}
        size="small"
      />
      <Button icon={<ReloadOutlined />} onClick={load} loading={loading} size="small">
        Refresh
      </Button>
    </div>
  );

  return (
    <div style={{ padding: '0 4px' }}>

      {/* ── Stat Cards ── */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        {[
          {
            title: 'Total Sessions',
            value: data?.totalSessions,
            Icon: BarChartOutlined,
            color: '#6366f1',
            suffix: undefined as string | undefined,
            extra: data ? `${data.learningSessions} learn / ${data.reviewSessions} review` : undefined,
          },
          {
            title: 'Completion Rate',
            value: data?.completionRate,
            Icon: CheckCircleOutlined,
            color: '#10b981',
            suffix: data ? '%' : undefined as string | undefined,
            extra: data ? `${data.completedSessions} completed` : undefined,
          },
          {
            title: 'Overall Accuracy',
            value: data?.overallAccuracy,
            Icon: QuestionCircleOutlined,
            color: '#3b82f6',
            suffix: data ? '%' : undefined as string | undefined,
            extra: data ? `${data.correctAnswers} / ${data.totalAnswers} correct` : undefined,
          },
          {
            title: 'Avg Response',
            value: data?.avgResponseTimeSeconds,
            Icon: ClockCircleOutlined,
            color: '#f59e0b',
            suffix: data ? 's' : undefined as string | undefined,
            extra: undefined as string | undefined,
          },
          {
            title: 'Hints Used',
            value: data?.totalHintsUsed,
            Icon: ThunderboltOutlined,
            color: '#e11d48',
            suffix: undefined as string | undefined,
            extra: undefined as string | undefined,
          },
          {
            title: 'Active Users',
            value: data?.topActiveUsers.length,
            Icon: UserOutlined,
            color: '#7c3aed',
            suffix: undefined as string | undefined,
            extra: `top ${data?.topActiveUsers.length ?? 0} shown`,
          },
        ].map(({ title, value, Icon, color, suffix, extra }) => (
          <Col xs={24} sm={12} md={8} key={title}>
            <Card variant="outlined">
              <Statistic
                title={title}
                value={value ?? '—'}
                suffix={suffix}
                prefix={<Icon style={{ color }} />}
                styles={{ content: { color } }}
              />
              {extra && <div style={{ fontSize: 11, color: '#6b7280', marginTop: 4 }}>{extra}</div>}
            </Card>
          </Col>
        ))}
      </Row>

      {/* ── Session Volume Chart ── */}
      <Card
        title={<span><BarChartOutlined style={{ marginRight: 6 }} />Daily Session Volume</span>}
        extra={controls}
        variant="outlined"
        style={{ marginBottom: 16 }}
      >
        {loading ? (
          <div style={{ textAlign: 'center', padding: 40 }}><Spin /></div>
        ) : (
          <ResponsiveContainer width="100%" height={260}>
            <AreaChart data={chartData} margin={{ top: 8, right: 12, left: 0, bottom: 0 }}>
              <defs>
                <linearGradient id="colorLearn" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%"  stopColor="#6366f1" stopOpacity={0.25} />
                  <stop offset="95%" stopColor="#6366f1" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="colorReview" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%"  stopColor="#f59e0b" stopOpacity={0.25} />
                  <stop offset="95%" stopColor="#f59e0b" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
              <XAxis dataKey="date" tick={{ fontSize: 11 }} tickFormatter={(d) => d.slice(5)} />
              <YAxis tick={{ fontSize: 11 }} allowDecimals={false} />
              <Tooltip
                labelFormatter={(d) => `Date: ${d}`}
                formatter={(val, name) => [val, name === 'learningSessions' ? 'Learning' : 'Review']}
              />
              <Legend formatter={(v) => v === 'learningSessions' ? 'Learning' : 'Review'} />
              <Area type="monotone" dataKey="learningSessions" stroke="#6366f1" fill="url(#colorLearn)"  strokeWidth={2} dot={false} />
              <Area type="monotone" dataKey="reviewSessions"   stroke="#f59e0b" fill="url(#colorReview)" strokeWidth={2} dot={false} />
            </AreaChart>
          </ResponsiveContainer>
        )}
      </Card>

      {/* ── Accuracy Trend Chart ── */}
      <Card
        title={<span><CheckCircleOutlined style={{ marginRight: 6 }} />Daily Accuracy Trend</span>}
        variant="outlined"
        style={{ marginBottom: 16 }}
      >
        {loading ? (
          <div style={{ textAlign: 'center', padding: 40 }}><Spin /></div>
        ) : (
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={chartData} margin={{ top: 8, right: 12, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
              <XAxis dataKey="date" tick={{ fontSize: 11 }} tickFormatter={(d) => d.slice(5)} />
              <YAxis tick={{ fontSize: 11 }} domain={[0, 100]} unit="%" />
              <Tooltip formatter={(val) => [`${val}%`, 'Accuracy']} labelFormatter={(d) => `Date: ${d}`} />
              <Line type="monotone" dataKey="accuracy" stroke="#10b981" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        )}
      </Card>

      {/* ── Top Active Users ── */}
      <Card
        title={<span><UserOutlined style={{ marginRight: 6 }} />Top Active Users (completed sessions)</span>}
        variant="outlined"
      >
        <Table<ActiveUser>
          columns={userColumns}
          dataSource={data?.topActiveUsers ?? []}
          rowKey="userId"
          loading={loading}
          pagination={false}
          size="small"
        />
      </Card>
    </div>
  );
}
