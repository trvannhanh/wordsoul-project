'use client';

import { useCallback, useEffect, useState } from 'react';
import {
  Button, Col, DatePicker, Drawer, Empty, Input, Row, Select,
  Spin, Table, Tag, Typography,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  CheckCircleOutlined, CloseCircleOutlined,
  EyeOutlined, FireOutlined, ReloadOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import dayjs from 'dayjs';

const { Text } = Typography;

// ── Types ────────────────────────────────────────────────────────────────────
interface BattleSession {
  id: number;
  challengerUsername: string;
  opponentUsername?: string;
  gymLeaderName?: string;
  type: string;
  status: string;
  startedAt: string;
  completedAt?: string;
  durationSeconds: number;
  totalQuestions: number;
  challengerCorrect: number;
  opponentCorrect: number;
  challengerWon?: boolean;
  roomCode?: string;
}

interface BattleSessionPage {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  items: BattleSession[];
}

interface BattleRound {
  roundIndex: number;
  vocabularyId: number;
  word: string;
  meaning: string;
  p1Score?: number;
  p1AnswerMs?: number;
  p1Correct: boolean;
  p1Answer?: string;
  p2Score?: number;
  p2AnswerMs?: number;
  p2Correct: boolean;
  p2Answer?: string;
  damageDealt: number;
  damagedPlayer: number;
  typeMultiplier: number;
}

interface BattleReplay extends BattleSession {
  rounds: BattleRound[];
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function fmtDuration(s: number) {
  const m = Math.floor(s / 60);
  const sec = s % 60;
  return m > 0 ? `${m}m ${sec}s` : `${sec}s`;
}

const TYPE_COLOR: Record<string, string>   = { GymBattle: 'blue', PvP: 'orange' };
const STATUS_COLOR: Record<string, string> = {
  Waiting: 'default', InProgress: 'processing', Completed: 'success', Abandoned: 'error',
};

// ── Columns ───────────────────────────────────────────────────────────────────
function buildColumns(onReplay: (id: number) => void): ColumnsType<BattleSession> {
  return [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      width: 56,
      render: (id) => <Text type="secondary" style={{ fontSize: 12 }}>#{id}</Text>,
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      width: 90,
      render: (t) => <Tag color={TYPE_COLOR[t] ?? 'default'}>{t}</Tag>,
    },
    {
      title: 'Challenger',
      dataIndex: 'challengerUsername',
      key: 'challenger',
      render: (name) => <Text strong style={{ fontSize: 12 }}>{name}</Text>,
    },
    {
      title: 'Opponent / Gym',
      key: 'opponent',
      render: (_, r) => {
        const name = r.opponentUsername ?? r.gymLeaderName;
        return name
          ? <Text style={{ fontSize: 12 }}>{name}</Text>
          : <Text type="secondary" style={{ fontSize: 12 }}>—</Text>;
      },
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (s) => <Tag color={STATUS_COLOR[s] ?? 'default'} style={{ fontSize: 11 }}>{s}</Tag>,
    },
    {
      title: 'Score',
      key: 'score',
      width: 80,
      align: 'center',
      render: (_, r) => (
        <Text style={{ fontSize: 12 }}>
          {r.challengerCorrect} / {r.opponentCorrect}
        </Text>
      ),
    },
    {
      title: 'Result',
      dataIndex: 'challengerWon',
      key: 'result',
      width: 70,
      align: 'center',
      render: (won) => {
        if (won == null) return <Text type="secondary" style={{ fontSize: 11 }}>—</Text>;
        return won
          ? <Tag color="green" style={{ fontSize: 11 }}>Win</Tag>
          : <Tag color="red"   style={{ fontSize: 11 }}>Loss</Tag>;
      },
    },
    {
      title: 'Duration',
      dataIndex: 'durationSeconds',
      key: 'duration',
      width: 72,
      align: 'center',
      render: (s) => <Text style={{ fontSize: 11 }}>{fmtDuration(s)}</Text>,
    },
    {
      title: 'Started',
      dataIndex: 'startedAt',
      key: 'startedAt',
      width: 120,
      render: (t) => <Text style={{ fontSize: 11 }}>{dayjs(t).format('MM-DD HH:mm')}</Text>,
    },
    {
      title: '',
      key: 'actions',
      width: 80,
      render: (_, r) => (
        <Button
          size="small"
          icon={<EyeOutlined />}
          onClick={() => onReplay(r.id)}
        >
          Replay
        </Button>
      ),
    },
  ];
}

// ── Round table columns ───────────────────────────────────────────────────────
const roundColumns: ColumnsType<BattleRound> = [
  {
    title: '#',
    dataIndex: 'roundIndex',
    key: 'roundIndex',
    width: 40,
    align: 'center',
    render: (i) => <Text style={{ fontSize: 11 }}>{i + 1}</Text>,
  },
  {
    title: 'Word',
    key: 'word',
    width: 100,
    render: (_, r) => (
      <div>
        <strong style={{ fontSize: 12 }}>{r.word}</strong>
        <div style={{ fontSize: 10, color: '#9ca3af', maxWidth: 90, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.meaning}</div>
      </div>
    ),
  },
  {
    title: 'P1 Answer',
    key: 'p1',
    width: 100,
    render: (_, r) => (
      <div>
        <span style={{ fontSize: 11 }}>{r.p1Answer ?? '—'}</span>
        {' '}
        {r.p1Correct
          ? <CheckCircleOutlined style={{ color: '#10b981', fontSize: 11 }} />
          : <CloseCircleOutlined style={{ color: '#ef4444', fontSize: 11 }} />}
        {r.p1AnswerMs != null && (
          <div style={{ fontSize: 10, color: '#9ca3af' }}>{r.p1AnswerMs}ms</div>
        )}
      </div>
    ),
  },
  {
    title: 'P2 Answer',
    key: 'p2',
    width: 100,
    render: (_, r) => (
      <div>
        <span style={{ fontSize: 11 }}>{r.p2Answer ?? '—'}</span>
        {' '}
        {r.p2Correct
          ? <CheckCircleOutlined style={{ color: '#10b981', fontSize: 11 }} />
          : <CloseCircleOutlined style={{ color: '#ef4444', fontSize: 11 }} />}
        {r.p2AnswerMs != null && (
          <div style={{ fontSize: 10, color: '#9ca3af' }}>{r.p2AnswerMs}ms</div>
        )}
      </div>
    ),
  },
  {
    title: 'Damage',
    key: 'damage',
    width: 90,
    align: 'center',
    render: (_, r) => {
      const who = r.damagedPlayer === 1 ? '→ P1' : r.damagedPlayer === 2 ? '→ P2' : 'Draw';
      return (
        <div style={{ fontSize: 11 }}>
          <strong style={{ color: '#ef4444' }}>{r.damageDealt}</strong>
          <span style={{ color: '#9ca3af', marginLeft: 4 }}>{who}</span>
          {r.typeMultiplier !== 1 && (
            <div style={{ fontSize: 10, color: '#f59e0b' }}>×{r.typeMultiplier.toFixed(1)}</div>
          )}
        </div>
      );
    },
  },
  {
    title: 'P1/P2 Score',
    key: 'score',
    width: 80,
    align: 'center',
    render: (_, r) => (
      <Text style={{ fontSize: 11 }}>{r.p1Score ?? 0} / {r.p2Score ?? 0}</Text>
    ),
  },
];

// ── Page ─────────────────────────────────────────────────────────────────────
export default function BattlesPage() {
  const [page, setPage]         = useState(1);
  const [pageSize]              = useState(20);
  const [data, setData]         = useState<BattleSessionPage | null>(null);
  const [loading, setLoading]   = useState(false);

  // Filters
  const [filterType,   setFilterType]   = useState<string | undefined>();
  const [filterStatus, setFilterStatus] = useState<string | undefined>();
  const [filterUserId, setFilterUserId] = useState('');

  // Replay drawer
  const [replayOpen,   setReplayOpen]   = useState(false);
  const [replay,       setReplay]       = useState<BattleReplay | null>(null);
  const [loadingReplay, setLoadingReplay] = useState(false);

  const load = useCallback(async (p = page) => {
    setLoading(true);
    try {
      const params: Record<string, unknown> = { page: p, pageSize };
      if (filterType)   params.type   = filterType;
      if (filterStatus) params.status = filterStatus;
      const uid = parseInt(filterUserId, 10);
      if (!isNaN(uid) && uid > 0) params.userId = uid;
      const res = await authApi.get(endpoints.adminBattles, { params });
      setData(res.data);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, filterType, filterStatus, filterUserId]);

  useEffect(() => { load(1); setPage(1); }, [filterType, filterStatus]);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => { load(); }, []);

  const openReplay = async (id: number) => {
    setReplayOpen(true);
    setReplay(null);
    setLoadingReplay(true);
    try {
      const res = await authApi.get(endpoints.adminBattleReplay(id));
      setReplay(res.data);
    } finally {
      setLoadingReplay(false);
    }
  };

  return (
    <div style={{ padding: '0 4px' }}>
      {/* ── Filter bar ── */}
      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginBottom: 16 }}>
        <Input
          placeholder="Filter by User ID"
          value={filterUserId}
          onChange={(e) => setFilterUserId(e.target.value)}
          onPressEnter={() => { load(1); setPage(1); }}
          style={{ width: 140 }}
          size="small"
          allowClear
        />
        <Select
          placeholder="Type"
          allowClear
          value={filterType}
          onChange={setFilterType}
          options={[{ label: 'Gym Battle', value: 'GymBattle' }, { label: 'PvP', value: 'PvP' }]}
          style={{ width: 120 }}
          size="small"
        />
        <Select
          placeholder="Status"
          allowClear
          value={filterStatus}
          onChange={setFilterStatus}
          options={[
            { label: 'Waiting',    value: 'Waiting' },
            { label: 'In Progress', value: 'InProgress' },
            { label: 'Completed',  value: 'Completed' },
            { label: 'Abandoned',  value: 'Abandoned' },
          ]}
          style={{ width: 130 }}
          size="small"
        />
        <Button
          icon={<ReloadOutlined />}
          size="small"
          onClick={() => { load(1); setPage(1); }}
          loading={loading}
        >
          Refresh
        </Button>
        {data && (
          <span style={{ fontSize: 12, color: '#6b7280', alignSelf: 'center' }}>
            {data.totalCount} battles
          </span>
        )}
      </div>

      {/* ── Table ── */}
      <Table<BattleSession>
        columns={buildColumns(openReplay)}
        dataSource={data?.items ?? []}
        rowKey="id"
        loading={loading}
        size="small"
        scroll={{ x: 900 }}
        pagination={{
          current: page,
          pageSize,
          total: data?.totalCount ?? 0,
          size: 'small',
          showSizeChanger: false,
          showTotal: (t) => `${t} total`,
          onChange: (p) => { setPage(p); load(p); },
        }}
      />

      {/* ── Replay Drawer ── */}
      <Drawer
        title={
          replay ? (
            <span>
              <FireOutlined style={{ marginRight: 6, color: '#f97316' }} />
              Battle #{replay.id}
              <Tag color={TYPE_COLOR[replay.type] ?? 'default'} style={{ marginLeft: 8 }}>{replay.type}</Tag>
              <Tag color={STATUS_COLOR[replay.status] ?? 'default'} style={{ marginLeft: 4 }}>{replay.status}</Tag>
            </span>
          ) : 'Battle Replay'
        }
        open={replayOpen}
        onClose={() => setReplayOpen(false)}
        width={760}
        styles={{ body: { padding: '16px 20px' } }}
      >
        {loadingReplay ? (
          <div style={{ textAlign: 'center', padding: 60 }}><Spin /></div>
        ) : !replay ? (
          <Empty />
        ) : (
          <>
            {/* ── Battle summary ── */}
            <Row gutter={16} style={{ marginBottom: 16 }}>
              <Col span={12}>
                <div style={{ background: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: 8, padding: '10px 14px' }}>
                  <div style={{ fontSize: 11, color: '#6b7280', marginBottom: 4 }}>CHALLENGER (P1)</div>
                  <div style={{ fontSize: 14, fontWeight: 700 }}>{replay.challengerUsername}</div>
                  <div style={{ fontSize: 20, fontWeight: 800, color: '#6366f1', marginTop: 4 }}>
                    {replay.challengerCorrect}
                    <span style={{ fontSize: 12, fontWeight: 400, color: '#6b7280', marginLeft: 4 }}>/ {replay.totalQuestions} correct</span>
                  </div>
                  {replay.challengerWon != null && (
                    <Tag color={replay.challengerWon ? 'green' : 'red'} style={{ marginTop: 6 }}>
                      {replay.challengerWon ? '🏆 Winner' : 'Defeated'}
                    </Tag>
                  )}
                </div>
              </Col>
              <Col span={12}>
                <div style={{ background: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: 8, padding: '10px 14px' }}>
                  <div style={{ fontSize: 11, color: '#6b7280', marginBottom: 4 }}>
                    {replay.type === 'PvP' ? 'OPPONENT (P2)' : 'GYM LEADER'}
                  </div>
                  <div style={{ fontSize: 14, fontWeight: 700 }}>
                    {replay.opponentUsername ?? replay.gymLeaderName ?? '—'}
                  </div>
                  <div style={{ fontSize: 20, fontWeight: 800, color: '#f97316', marginTop: 4 }}>
                    {replay.opponentCorrect}
                    <span style={{ fontSize: 12, fontWeight: 400, color: '#6b7280', marginLeft: 4 }}>/ {replay.totalQuestions} correct</span>
                  </div>
                  {replay.challengerWon != null && (
                    <Tag color={!replay.challengerWon ? 'green' : 'red'} style={{ marginTop: 6 }}>
                      {!replay.challengerWon ? '🏆 Winner' : 'Defeated'}
                    </Tag>
                  )}
                </div>
              </Col>
            </Row>

            {/* ── Meta row ── */}
            <div style={{ display: 'flex', gap: 16, fontSize: 12, color: '#6b7280', marginBottom: 16, flexWrap: 'wrap' }}>
              <span>Started: <strong>{dayjs(replay.startedAt).format('YYYY-MM-DD HH:mm')}</strong></span>
              <span>Duration: <strong>{fmtDuration(replay.durationSeconds)}</strong></span>
              {replay.roomCode && <span>Room: <strong>{replay.roomCode}</strong></span>}
            </div>

            {/* ── Round-by-round table ── */}
            <div style={{ fontSize: 12, fontWeight: 600, color: '#374151', marginBottom: 8 }}>
              Round-by-Round ({replay.rounds.length} rounds)
            </div>
            {replay.rounds.length === 0 ? (
              <Empty description="No round data" image={Empty.PRESENTED_IMAGE_SIMPLE} />
            ) : (
              <Table<BattleRound>
                columns={roundColumns}
                dataSource={replay.rounds}
                rowKey="roundIndex"
                size="small"
                pagination={false}
                scroll={{ x: 560 }}
                rowClassName={(r) =>
                  r.damagedPlayer === 1 ? 'row-p1-hit' : r.damagedPlayer === 2 ? 'row-p2-hit' : ''
                }
              />
            )}
          </>
        )}
      </Drawer>
    </div>
  );
}
