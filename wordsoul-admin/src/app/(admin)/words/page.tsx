'use client';

import React, { useState, useCallback, useEffect } from 'react';
import {
  Table, Button, Input, Select, Modal, Form, App, Space, Popconfirm, Upload, Tag,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined, UploadOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import ImageUploader from '../_components/ImageUploader';

interface Vocabulary {
  id: number;
  word: string;
  meaning: string;
  imageUrl?: string;
  pronunciation?: string;
  partOfSpeech?: string;
  cefrLevel?: string;
  description?: string;
  exampleSentence?: string;
}

const POS_OPTIONS = [
  'Noun', 'Verb', 'Adjective', 'Adverb', 'Pronoun',
  'Preposition', 'Conjunction', 'Interjection',
];
const CEFR_OPTIONS = ['A1', 'A2', 'B1', 'B2', 'C1', 'C2'];

const POS_COLOR: Record<string, string> = {
  Noun: 'blue', Verb: 'green', Adjective: 'orange', Adverb: 'purple',
};

export default function WordsPage() {
  const { message } = App.useApp();

  const [words, setWords] = useState<Vocabulary[]>([]);
  const [loading, setLoading] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [posFilter, setPosFilter] = useState<string | undefined>();

  const [modalOpen, setModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [editing, setEditing] = useState<Vocabulary | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [form] = Form.useForm();

  const fetchWords = useCallback(async () => {
    setLoading(true);
    try {
      const params: Record<string, unknown> = { pageNumber, pageSize };
      if (search) params.word = search;
      if (posFilter) params.partOfSpeech = posFilter;
      const { data } = await authApi.get(endpoints.vocabularies, { params });
      // API may return array or paged result
      if (Array.isArray(data)) {
        setWords(data);
        setTotal(data.length);
      } else {
        setWords(data.items ?? data);
        setTotal(data.totalCount ?? (data.items ?? data).length);
      }
    } catch {
      message.error('Failed to load words');
    } finally {
      setLoading(false);
    }
  }, [pageNumber, pageSize, search, posFilter, message]);

  useEffect(() => { fetchWords(); }, [fetchWords]);

  const openCreate = () => {
    setModalMode('create');
    setEditing(null);
    setSelectedFile(null);
    form.resetFields();
    setModalOpen(true);
  };

  const openEdit = (record: Vocabulary) => {
    setModalMode('edit');
    setEditing(record);
    setSelectedFile(null);
    form.setFieldsValue({
      word: record.word,
      meaning: record.meaning,
      pronunciation: record.pronunciation,
      partOfSpeech: record.partOfSpeech,
      cefrLevel: record.cefrLevel,
      description: record.description,
      exampleSentence: record.exampleSentence,
    });
    setModalOpen(true);
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);
      const fd = new FormData();
      fd.append('Word', values.word);
      fd.append('Meaning', values.meaning);
      if (values.pronunciation) fd.append('Pronunciation', values.pronunciation);
      if (values.partOfSpeech) fd.append('PartOfSpeech', values.partOfSpeech);
      if (values.cefrLevel) fd.append('CEFRLevel', values.cefrLevel);
      if (values.description) fd.append('Description', values.description);
      if (values.exampleSentence) fd.append('ExampleSentence', values.exampleSentence);
      if (selectedFile) fd.append('ImageFile', selectedFile);

      if (modalMode === 'create') {
        await authApi.post(endpoints.vocabularies, fd, { headers: { 'Content-Type': 'multipart/form-data' } });
        message.success('Word created');
      } else if (editing) {
        await authApi.put(`${endpoints.vocabularies}/${editing.id}`, fd, { headers: { 'Content-Type': 'multipart/form-data' } });
        message.success('Word updated');
      }
      setModalOpen(false);
      fetchWords();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'errorFields' in err) return;
      message.error('Failed to save word');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await authApi.delete(`${endpoints.vocabularies}/${id}`);
      message.success('Word deleted');
      fetchWords();
    } catch {
      message.error('Failed to delete word');
    }
  };

  const columns: ColumnsType<Vocabulary> = [
    {
      title: 'Image',
      dataIndex: 'imageUrl',
      key: 'imageUrl',
      width: 56,
      render: (url) => url
        ? <img src={url} alt="" style={{ width: 36, height: 36, borderRadius: 6, objectFit: 'cover', border: '1px solid var(--border)' }} />
        : <div style={{ width: 36, height: 36, borderRadius: 6, background: 'var(--bg-muted)', border: '1px solid var(--border)' }} />,
    },
    {
      title: 'Word',
      dataIndex: 'word',
      key: 'word',
      render: (w: string) => <span style={{ fontWeight: 600, fontSize: 13 }}>{w}</span>,
    },
    {
      title: 'Meaning',
      dataIndex: 'meaning',
      key: 'meaning',
      ellipsis: true,
      render: (m: string) => <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{m}</span>,
    },
    {
      title: 'Pronunciation',
      dataIndex: 'pronunciation',
      key: 'pronunciation',
      width: 140,
      render: (v?: string) => <span style={{ fontSize: 12, color: 'var(--text-muted)', fontStyle: 'italic' }}>{v || '—'}</span>,
    },
    {
      title: 'Part of Speech',
      dataIndex: 'partOfSpeech',
      key: 'partOfSpeech',
      width: 120,
      render: (v?: string) => v
        ? <Tag color={POS_COLOR[v] ?? 'default'} style={{ fontSize: 11 }}>{v}</Tag>
        : <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>—</span>,
    },
    {
      title: 'CEFR',
      dataIndex: 'cefrLevel',
      key: 'cefrLevel',
      width: 70,
      render: (v?: string) => <span style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-muted)' }}>{v || '—'}</span>,
    },
    {
      title: '',
      key: 'actions',
      width: 90,
      render: (_, record) => (
        <Space size={4}>
          <Button size="small" icon={<EditOutlined />} onClick={() => openEdit(record)} />
          <Popconfirm
            title={`Delete "${record.word}"?`}
            okText="Delete"
            okButtonProps={{ danger: true }}
            onConfirm={() => handleDelete(record.id)}
          >
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Vocabulary Words</h1>
          <p className="page-subtitle">Manage individual vocabulary words independent of any set.</p>
        </div>
        <Button type="primary" size="small" icon={<PlusOutlined />} onClick={openCreate}>
          Add Word
        </Button>
      </div>

      <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
        <div style={{ padding: '12px 16px', borderBottom: '1px solid var(--border)', display: 'flex', gap: 8, flexWrap: 'wrap', alignItems: 'center' }}>
          <Input
            placeholder="Search by word…"
            allowClear
            value={searchInput}
            onChange={e => setSearchInput(e.target.value)}
            onPressEnter={() => { setSearch(searchInput); setPageNumber(1); }}
            onClear={() => { setSearchInput(''); setSearch(''); setPageNumber(1); }}
            style={{ maxWidth: 220 }}
            size="small"
            prefix={<SearchOutlined style={{ color: 'var(--text-muted)' }} />}
          />
          <Select
            placeholder="Part of speech"
            allowClear
            size="small"
            style={{ width: 150 }}
            onChange={(v) => { setPosFilter(v); setPageNumber(1); }}
            options={POS_OPTIONS.map(p => ({ value: p, label: p }))}
          />
          <span style={{ fontSize: 12, color: 'var(--text-muted)', marginLeft: 'auto' }}>
            {total} word{total !== 1 ? 's' : ''}
          </span>
        </div>

        <Table
          rowKey="id"
          columns={columns}
          dataSource={words}
          loading={loading}
          size="small"
          pagination={{
            current: pageNumber,
            pageSize,
            total,
            showSizeChanger: true,
            showTotal: (t) => `${t} words`,
            onChange: (p, s) => { setPageNumber(p); setPageSize(s); },
            style: { padding: '8px 16px' },
          }}
        />
      </div>

      <Modal
        title={modalMode === 'create' ? 'Add Word' : 'Edit Word'}
        open={modalOpen}
        onOk={handleSubmit}
        onCancel={() => { setModalOpen(false); form.resetFields(); }}
        confirmLoading={submitting}
        okText={modalMode === 'create' ? 'Create' : 'Save'}
        width={480}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" style={{ marginTop: 12 }}>
          <Form.Item label="Word Image" style={{ marginBottom: 12 }}>
            <ImageUploader
              value={modalMode === 'edit' ? editing?.imageUrl : undefined}
              onChange={setSelectedFile}
            />
          </Form.Item>
          <Form.Item name="word" label="Word" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="meaning" label="Meaning" rules={[{ required: true }]}>
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="pronunciation" label="Pronunciation">
            <Input placeholder="e.g. /prəˌnʌnsiˈeɪʃən/" />
          </Form.Item>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <Form.Item name="partOfSpeech" label="Part of Speech" style={{ marginBottom: 0 }}>
              <Select options={POS_OPTIONS.map(p => ({ value: p, label: p }))} allowClear />
            </Form.Item>
            <Form.Item name="cefrLevel" label="CEFR Level" style={{ marginBottom: 0 }}>
              <Select options={CEFR_OPTIONS.map(c => ({ value: c, label: c }))} allowClear />
            </Form.Item>
          </div>
          <Form.Item name="description" label="Description" style={{ marginTop: 12 }}>
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="exampleSentence" label="Example Sentence">
            <Input.TextArea rows={2} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
