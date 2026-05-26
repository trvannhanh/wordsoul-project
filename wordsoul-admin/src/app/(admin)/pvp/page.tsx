'use client';

import React, { useCallback, useEffect, useState } from 'react';
import {
  Avatar, Button, Card, Col, Row, Select, Statistic, Table, Tag, Typography,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  ReloadOutlined, ThunderboltOutlined, TrophyOutlined, UserOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

const { Text } = Typography;

// ── Types ────────────────────────────────────────────────────────────────────
interface PvpEntry {
  rank: number;
  userId: number;
  userName: string;
  avatarUrl?: string;
  pvpRating: number;
  wins: number;
  losses: number;
  totalGames: number;
  winRate: number;
}

interface PvpLeaderboard {
  entries: PvpEntry[];
  totalActivePlayers: number;
  averageRating: number;
  highestRating: number;
}

// ── Helpers ──────────────────────────────────────────────────────────────────
function getRatingColor(rating: number): string {
  if (rating >= 1800) return '#f59e0b'; // Gold
  if (rating >= 1500) return '#6366f1'; // Purple
  if (rating >= 1200) return '#3b82f6'; // Blue
  if (rating >= 1000) return '#10b981'; // Green
  return '#6b7280';                     // Gray
}

function getRatingLabel(rating: number): string {
  if (rating >= 1800) return 'Grandmaster';
  if (rating >= 1500) return 'Master';
  if (rating >= 1200) return 'Diamond';
  if (rating >= 1000) return 'Gold';
  return 'Bronze';
}

function RankMedal({ rank }: { rank: number }) {
  if (rank === 1) return <span style={{ fontSize: 20 }}>🥇</span>;
  if (rank === 2) return <span style={{ fontSize: 20 }}>🥈</span>;
  if (rank === 3) return <span style={{ fontSize: 20 }}>🥉</span>;
  return <Text type="secondary" style={{ fontWeight: 600 }}>#{rank}</Text>;
}

// ── Columns ──────────────────────────────────────────────────────────────────
const columns: ColumnsType<PvpEntry> = [
  {
    title: 'Rank',
    dataIndex: 'rank',
    key: 'rank',
    width: 70,
    align: 'center',
    render: (rank) => <RankMedal rank={rank} />,
  },
  {
    title: 'Player',
    key: 'player',
    render: (_, row) => (
      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
        {row.avatarUrl
          ? <Avatar src={row.avatarUrl} size={36} />
          : <Avatar icon={<UserOutlined />} size={36} style={{ background: '#6366f1' }} />}
        <Text strong>{row.userName}</Text>
      </div>
    ),
  },
  {
    title: 'Rating',
    dataIndex: 'pvpRating',
    key: 'pvpRating',
    width: 160,
    render: (rating) => (
      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
        <Text strong style={{ color: getRatingColor(rating), fontSize: 16 }}>{rating}</Text>
        <Tag color={getRatingColor(rating)} style={{ margin: 0, fontSize: 11 }}>
          {getRatingLabel(rating)}
        </Tag>
      </div>
    ),
    sorter: (a, b) => a.pvpRating - b.pvpRating,
    defaultSortOrder: 'descend',
  },
  {
    title: 'W / L',
    key: 'wl',
    width: 100,
    render: (_, row) => (
      <span>
        <Text style={{ color: '#10b981', fontWeight: 600 }}>{row.wins}</Text>
        <Text type="secondary"> / </Text>
        <Text style={{ color: '#ef4444', fontWeight: 600 }}>{row.losses}</Text>
      </span>
    ),
  },
  {
    title: 'Games',
    dataIndex: 'totalGames',
    key: 'totalGames',
    width: 80,
    align: 'center',
  },
  {
    title: 'Win Rate',
    dataIndex: 'winRate',
    key: 'winRate',
    width: 110,
    align: 'center',
    render: (wr) => {
      const color = wr >= 60 ? '#10b981' : wr >= 45 ? '#3b82f6' : '#ef4444';
      return <Text strong style={{ color }}>{wr.toFixed(1)}%</Text>;
    },
    sorter: (a, b) => a.winRate - b.winRate,
  },
];

// ── Page ─────────────────────────────────────────────────────────────────────
const TOP_OPTIONS = [
  { label: 'Top 10',  value: 10 },
  { label: 'Top 25',  value: 25 },
  { label: 'Top 50',  value: 50 },
  { label: 'Top 100', value: 100 },
];

export default function PvpLeaderboardPage() {
  const [data, setData]     = useState<PvpLeaderboard | null>(null);
  const [loading, setLoading] = useState(false);
  const [top, setTop]       = useState(50);

  const fetch = useCallback(async () => {
    setLoading(true);
    try {
      const res = await authApi.get(endpoints.pvpLeaderboard, { params: { top } });
      setData(res.data);
    } finally {
      setLoading(false);
    }
  }, [top]);

  useEffect(() => { fetch(); }, [fetch]);

  return (
    <div style={{ padding: '0 4px' }}>
      {/* ── Stat Cards ── */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={8}>
          <Card variant="outlined">
            <Statistic
              title="Active Players"
              value={data?.totalActivePlayers ?? '—'}
              prefix={<ThunderboltOutlined style={{ color: '#6366f1' }} />}
              styles={{ content: { color: '#6366f1' } }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card variant="outlined">
            <Statistic
              title="Average Rating"
              value={data?.averageRating ?? '—'}
              prefix={<TrophyOutlined style={{ color: '#f59e0b' }} />}
              styles={{ content: { color: '#f59e0b' } }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card variant="outlined">
            <Statistic
              title="Highest Rating"
              value={data?.highestRating ?? '—'}
              prefix={<TrophyOutlined style={{ color: '#ef4444' }} />}
              styles={{ content: { color: '#ef4444' } }}
            />
          </Card>
        </Col>
      </Row>

      {/* ── Leaderboard Table ── */}
      <Card
        title={
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <ThunderboltOutlined />
            <span>PvP Leaderboard</span>
          </div>
        }
        extra={
          <div style={{ display: 'flex', gap: 8 }}>
            <Select
              value={top}
              onChange={(v) => setTop(v)}
              options={TOP_OPTIONS}
              style={{ width: 100 }}
              size="small"
            />
            <Button
              icon={<ReloadOutlined />}
              onClick={fetch}
              loading={loading}
              size="small"
            >
              Refresh
            </Button>
          </div>
        }
        variant="outlined"
      >
        <Table<PvpEntry>
          columns={columns}
          dataSource={data?.entries ?? []}
          rowKey="userId"
          loading={loading}
          pagination={false}
          size="middle"
          rowClassName={(row) =>
            row.rank <= 3 ? 'pvp-top3-row' : ''
          }
        />
      </Card>
    </div>
  );
}
