'use client';

import React, { useState, useEffect, useCallback } from 'react';
import {
  Table, Input, Select, Button, Tag, App,
  Popconfirm, Space, Tooltip,
} from 'antd';
import {
  PlusOutlined, EyeOutlined, DeleteOutlined,
  RobotOutlined, SearchOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import dayjs from 'dayjs';
import { useRouter } from 'next/navigation';
import AiGenerationModal from '@/components/AiGenerationModal';

interface VocabularySet {
  id: number;
  title: string;
  theme: string;
  description: string;
  imageUrl: string;
  difficultyLevel: string;
  createdAt: string;
  isActive: boolean;
  isPublic: boolean;
  createdByUsername: string;
  totalVocabularies?: number;
}

const DIFFICULTY_STYLE: Record<string, { color: string; bg: string; border: string }> = {
  Easy:   { color: '#15803D', bg: '#F0FDF4', border: '#BBF7D0' },
  Medium: { color: '#B45309', bg: '#FFFBEB', border: '#FDE68A' },
  Hard:   { color: '#B91C1C', bg: '#FEF2F2', border: '#FECACA' },
};

function DifficultyTag({ level }: { level: string }) {
  const base = level?.split(' ')[0] ?? 'Easy';
  const s = DIFFICULTY_STYLE[base] ?? DIFFICULTY_STYLE['Easy'];
  return (
    <span style={{ fontSize: 11, fontWeight: 600, padding: '2px 7px', borderRadius: 4, color: s.color, background: s.bg, border: `1px solid ${s.border}` }}>
      {level || 'Easy'}
    </span>
  );
}

export default function VocabularySetsPage() {
  const [sets, setSets] = useState<VocabularySet[]>([]);
  const [loading, setLoading] = useState(false);
  const [isAiModalVisible, setIsAiModalVisible] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [diffFilter, setDiffFilter] = useState<string | null>(null);
  const [visFilter, setVisFilter] = useState<string | null>(null);
  const router = useRouter();
  const { message } = App.useApp();

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(15);
  const [hasMore, setHasMore] = useState(true);

  const fetchSets = useCallback(async (page: number, size: number) => {
    setLoading(true);
    try {
      const response = await authApi.get(
        `${endpoints.vocabularySets}?pageNumber=${page}&pageSize=${size}`
      );
      setSets(response.data);
      setHasMore(response.data.length === size);
    } catch {
      message.error('Failed to fetch vocabulary sets');
    } finally {
      setLoading(false);
    }
  }, [message]);

  useEffect(() => { fetchSets(currentPage, pageSize); }, [currentPage, pageSize, fetchSets]);

  const handleDelete = async (id: number) => {
    try {
      await authApi.delete(`${endpoints.vocabularySets}/${id}`);
      message.success('Set deleted');
      fetchSets(currentPage, pageSize);
    } catch { message.error('Failed to delete set'); }
  };

  // Client-side filtering
  const filtered = sets.filter(s => {
    const matchSearch = !searchText || s.title.toLowerCase().includes(searchText.toLowerCase());
    const matchDiff = !diffFilter || (s.difficultyLevel ?? '').startsWith(diffFilter);
    const matchVis = !visFilter || (visFilter === 'public' ? s.isPublic : !s.isPublic);
    return matchSearch && matchDiff && matchVis;
  });

  const columns = [
    {
      title: 'Cover',
      dataIndex: 'imageUrl',
      key: 'imageUrl',
      width: 54,
      render: (url: string) => url
        ? <img src={url} alt="" style={{ width: 36, height: 36, borderRadius: 6, objectFit: 'cover', border: '1px solid var(--border)' }} />
        : <div style={{ width: 36, height: 36, borderRadius: 6, background: 'var(--bg-muted)', border: '1px solid var(--border)' }} />,
    },
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
      render: (text: string, record: VocabularySet) => (
        <div>
          <button
            onClick={() => router.push(`/vocabularies/${record.id}`)}
            style={{ background: 'none', border: 'none', padding: 0, cursor: 'pointer', textAlign: 'left' }}
          >
            <span style={{ fontSize: 13, fontWeight: 500, color: 'var(--accent)', textDecoration: 'none' }}>
              {text}
            </span>
          </button>
          {record.description && (
            <div style={{ fontSize: 11, color: 'var(--text-muted)', marginTop: 1 }}>
              {record.description.length > 50 ? record.description.slice(0, 50) + '…' : record.description}
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Theme',
      dataIndex: 'theme',
      key: 'theme',
      width: 100,
      render: (theme: string) => (
        <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{theme}</span>
      ),
    },
    {
      title: 'Difficulty',
      dataIndex: 'difficultyLevel',
      key: 'difficultyLevel',
      width: 90,
      render: (level: string) => <DifficultyTag level={level} />,
    },
    {
      title: 'Words',
      dataIndex: 'totalVocabularies',
      key: 'totalVocabularies',
      width: 64,
      render: (n: number) => (
        <span style={{ fontSize: 12, color: 'var(--text-secondary)', fontVariantNumeric: 'tabular-nums' }}>
          {n ?? '—'}
        </span>
      ),
    },
    {
      title: 'Visibility',
      dataIndex: 'isPublic',
      key: 'isPublic',
      width: 80,
      render: (isPublic: boolean) => (
        <span style={{ fontSize: 11, color: isPublic ? 'var(--success)' : 'var(--text-muted)' }}>
          {isPublic ? 'Public' : 'Private'}
        </span>
      ),
    },
    {
      title: 'Created',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 100,
      render: (d: string) => (
        <span style={{ fontSize: 12, color: 'var(--text-muted)', fontVariantNumeric: 'tabular-nums' }}>
          {dayjs(d).format('YYYY-MM-DD')}
        </span>
      ),
    },
    {
      title: '',
      key: 'action',
      width: 72,
      render: (_: any, record: VocabularySet) => (
        <Space size={0}>
          <Tooltip title="View details">
            <Button
              type="text"
              size="small"
              icon={<EyeOutlined />}
              onClick={() => router.push(`/vocabularies/${record.id}`)}
              style={{ color: 'var(--text-muted)' }}
            />
          </Tooltip>
          <Popconfirm
            title="Delete this set?"
            onConfirm={() => handleDelete(record.id)}
            okText="Delete"
            okButtonProps={{ danger: true }}
            cancelText="Cancel"
          >
            <Button type="text" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Vocabulary Library</h1>
          <p className="page-subtitle">Manage vocabulary sets, content, and AI generation.</p>
        </div>
        <Space size={8}>
          <Button size="small" icon={<PlusOutlined />}>
            Create Set
          </Button>
          <Button
            type="primary"
            size="small"
            icon={<RobotOutlined />}
            onClick={() => setIsAiModalVisible(true)}
          >
            AI Generate
          </Button>
        </Space>
      </div>

      {/* Main Card */}
      <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
        {/* Filter Bar */}
        <div style={{ padding: '12px 16px', borderBottom: '1px solid var(--border)', display: 'flex', gap: 8, flexWrap: 'wrap', alignItems: 'center' }}>
          <Input
            placeholder="Search by title…"
            allowClear
            value={searchText}
            onChange={e => setSearchText(e.target.value)}
            style={{ maxWidth: 240 }}
            size="small"
            prefix={<SearchOutlined style={{ color: 'var(--text-muted)' }} />}
          />
          <Select
            placeholder="Difficulty"
            allowClear
            size="small"
            style={{ width: 120 }}
            onChange={setDiffFilter}
            options={[
              { value: 'Easy', label: 'Easy' },
              { value: 'Medium', label: 'Medium' },
              { value: 'Hard', label: 'Hard' },
            ]}
          />
          <Select
            placeholder="Visibility"
            allowClear
            size="small"
            style={{ width: 110 }}
            onChange={setVisFilter}
            options={[
              { value: 'public', label: 'Public' },
              { value: 'private', label: 'Private' },
            ]}
          />
          <span style={{ fontSize: 12, color: 'var(--text-muted)', marginLeft: 'auto' }}>
            {filtered.length} set{filtered.length !== 1 ? 's' : ''}
          </span>
        </div>

        <Table
          columns={columns}
          dataSource={filtered}
          rowKey="id"
          loading={loading}
          size="small"
          pagination={{
            current: currentPage,
            pageSize,
            total: hasMore ? currentPage * pageSize + 1 : currentPage * pageSize,
            onChange: (p, s) => { setCurrentPage(p); setPageSize(s); },
            showSizeChanger: true,
            style: { padding: '8px 16px' },
          }}
        />
      </div>

      <AiGenerationModal
        open={isAiModalVisible}
        onCancel={() => setIsAiModalVisible(false)}
        onSuccess={() => { setIsAiModalVisible(false); fetchSets(currentPage, pageSize); }}
      />
    </div>
  );
}
