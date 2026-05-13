'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import {
  Button, Table, Space, Tag, Popconfirm,
  Modal, Form, Input, Select, Upload, App, Descriptions, Badge,
} from 'antd';
import {
  ArrowLeftOutlined, EditOutlined, DeleteOutlined,
  PlusOutlined, UploadOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import dayjs from 'dayjs';

interface Vocabulary {
  id: number;
  word: string;
  meaning: string;
  imageUrl: string;
  pronunciation: string;
  partOfSpeech: string;
}

interface VocabularySetDetail {
  id: number;
  title: string;
  theme: string;
  imageUrl: string;
  description: string;
  difficultyLevel: string;
  isActive: boolean;
  createdAt: string;
  vocabularies: Vocabulary[];
  totalVocabularies: number;
  currentPage: number;
  pageSize: number;
}

const metaItem = (label: string, value: React.ReactNode) => (
  <div style={{ marginBottom: 10 }}>
    <div style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: 3 }}>
      {label}
    </div>
    <div style={{ fontSize: 13, color: 'var(--text-primary)' }}>{value}</div>
  </div>
);

export default function VocabularySetDetailPage() {
  const params = useParams();
  const id = params.id as string;
  const router = useRouter();

  const [data, setData] = useState<VocabularySetDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [vocabLoading, setVocabLoading] = useState(false);

  const [isEditSetModalOpen, setIsEditSetModalOpen] = useState(false);
  const [isVocabModalOpen, setIsVocabModalOpen] = useState(false);
  const [editingVocab, setEditingVocab] = useState<Vocabulary | null>(null);

  const [form] = Form.useForm();
  const [vocabForm] = Form.useForm();
  const { message } = App.useApp();

  const fetchDetails = async () => {
    setLoading(true);
    try {
      const response = await authApi.get(endpoints.vocabularySetDetail(Number(id)));
      setData(response.data);
    } catch { message.error('Failed to fetch set details'); }
    finally { setLoading(false); }
  };

  useEffect(() => { if (id) fetchDetails(); }, [id]);

  const handleUpdateSet = async (values: any) => {
    try {
      await authApi.put(`${endpoints.vocabularySets}/${id}`, values);
      message.success('Set updated');
      setIsEditSetModalOpen(false);
      fetchDetails();
    } catch { message.error('Failed to update set'); }
  };

  const handleRemoveVocab = async (vocabId: number) => {
    try {
      await authApi.delete(`/vocabularies/${id}/vocabularies/${vocabId}`);
      message.success('Vocabulary removed');
      fetchDetails();
    } catch { message.error('Failed to remove vocabulary'); }
  };

  const handleSaveVocab = async (values: any) => {
    setVocabLoading(true);
    try {
      const formData = new FormData();
      formData.append('Word', values.word);
      formData.append('Meaning', values.meaning);
      formData.append('Pronunciation', values.pronunciation || '');
      formData.append('PartOfSpeech', values.partOfSpeech || '');
      if (values.imageFile?.[0]?.originFileObj) {
        formData.append('ImageFile', values.imageFile[0].originFileObj);
      }
      if (editingVocab) {
        await authApi.put(`${endpoints.vocabularies}/${editingVocab.id}`, formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
        message.success('Vocabulary updated');
      } else {
        const res = await authApi.post(endpoints.vocabularies, formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
        await authApi.post(`/vocabularies/${id}/vocabularies`, { vocabularyId: res.data.id });
        message.success('Vocabulary added');
      }
      setIsVocabModalOpen(false);
      fetchDetails();
    } catch { message.error('Failed to save vocabulary'); }
    finally { setVocabLoading(false); }
  };

  const openEditVocabModal = (vocab: Vocabulary) => {
    setEditingVocab(vocab);
    vocabForm.setFieldsValue(vocab);
    setIsVocabModalOpen(true);
  };

  const openAddVocabModal = () => {
    setEditingVocab(null);
    vocabForm.resetFields();
    setIsVocabModalOpen(true);
  };

  const wordColumns = [
    {
      title: '',
      dataIndex: 'imageUrl',
      width: 48,
      render: (url: string) => url
        ? <img src={url} alt="" style={{ width: 32, height: 32, objectFit: 'cover', borderRadius: 4, border: '1px solid var(--border)' }} />
        : <div style={{ width: 32, height: 32, borderRadius: 4, background: 'var(--bg-muted)', border: '1px solid var(--border)' }} />,
    },
    {
      title: 'Word',
      dataIndex: 'word',
      render: (w: string) => <span style={{ fontWeight: 600, fontSize: 13, color: 'var(--text-primary)' }}>{w}</span>,
    },
    {
      title: 'Type',
      dataIndex: 'partOfSpeech',
      width: 90,
      render: (v: string) => v ? <Tag style={{ fontSize: 11 }}>{v}</Tag> : null,
    },
    {
      title: 'Pronunciation',
      dataIndex: 'pronunciation',
      width: 130,
      render: (v: string) => <span style={{ fontSize: 12, color: 'var(--text-muted)', fontFamily: 'monospace' }}>{v}</span>,
    },
    {
      title: 'Meaning',
      dataIndex: 'meaning',
      render: (v: string) => <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{v}</span>,
    },
    {
      title: '',
      key: 'action',
      width: 72,
      render: (_: any, record: Vocabulary) => (
        <Space size={0}>
          <Button type="text" size="small" icon={<EditOutlined />} onClick={() => openEditVocabModal(record)} style={{ color: 'var(--text-muted)' }} />
          <Popconfirm title="Remove from set?" onConfirm={() => handleRemoveVocab(record.id)} okText="Remove" okButtonProps={{ danger: true }}>
            <Button type="text" size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  if (loading) return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 200, color: 'var(--text-muted)', fontSize: 13 }}>
      Loading…
    </div>
  );
  if (!data) return <div style={{ color: 'var(--text-muted)', fontSize: 13 }}>Set not found.</div>;

  return (
    <div>
      {/* Page Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 16 }}>
        <Button
          type="text"
          size="small"
          icon={<ArrowLeftOutlined />}
          onClick={() => router.push('/vocabularies')}
          style={{ color: 'var(--text-muted)' }}
        />
        <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>Vocabulary Library /</span>
        <span style={{ fontSize: 12, color: 'var(--text-primary)', fontWeight: 500 }}>{data.title}</span>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '220px 1fr', gap: 16, alignItems: 'start' }}>
        {/* Left: Set info */}
        <div>
          <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
            <img
              src={data.imageUrl || 'https://via.placeholder.com/220x120?text=No+Cover'}
              alt="cover"
              style={{ width: '100%', height: 110, objectFit: 'cover', display: 'block' }}
            />
            <div style={{ padding: '14px 16px' }}>
              <div style={{ fontSize: 14, fontWeight: 600, color: 'var(--text-primary)', marginBottom: 10 }}>{data.title}</div>
              {data.description && (
                <div style={{ fontSize: 12, color: 'var(--text-secondary)', marginBottom: 12, lineHeight: 1.5 }}>{data.description}</div>
              )}
              {metaItem('Theme', data.theme)}
              {metaItem('Difficulty', data.difficultyLevel)}
              {metaItem('Created', dayjs(data.createdAt).format('YYYY-MM-DD'))}
              {metaItem('Status', (
                <span style={{ display: 'inline-flex', alignItems: 'center', gap: 5 }}>
                  <span style={{ width: 6, height: 6, borderRadius: '50%', background: data.isActive ? 'var(--success)' : 'var(--text-muted)', display: 'inline-block' }} />
                  <span style={{ fontSize: 12 }}>{data.isActive ? 'Active' : 'Inactive'}</span>
                </span>
              ))}
              <Button
                size="small"
                icon={<EditOutlined />}
                style={{ width: '100%', marginTop: 8 }}
                onClick={() => { form.setFieldsValue(data); setIsEditSetModalOpen(true); }}
              >
                Edit Set Info
              </Button>
            </div>
          </div>
        </div>

        {/* Right: Words table */}
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
          <div style={{ padding: '12px 16px', borderBottom: '1px solid var(--border)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>
              Words in Set
              <span style={{ fontSize: 12, fontWeight: 400, color: 'var(--text-muted)', marginLeft: 6 }}>
                ({data.totalVocabularies})
              </span>
            </span>
            <Button type="primary" size="small" icon={<PlusOutlined />} onClick={openAddVocabModal}>
              Add Word
            </Button>
          </div>
          <Table
            dataSource={data.vocabularies}
            columns={wordColumns}
            rowKey="id"
            pagination={false}
            size="small"
          />
        </div>
      </div>

      {/* Edit Set Modal */}
      <Modal
        title="Edit Vocabulary Set"
        open={isEditSetModalOpen}
        onCancel={() => setIsEditSetModalOpen(false)}
        onOk={() => form.submit()}
        okText="Save"
        width={420}
      >
        <Form form={form} layout="vertical" onFinish={handleUpdateSet} style={{ marginTop: 16 }}>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="theme" label="Theme" rules={[{ required: true }]}>
            <Select>
              {['Academic', 'Casual', 'TOEIC', 'IELTS', 'Travel'].map(t => (
                <Select.Option key={t} value={t}>{t}</Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item name="difficultyLevel" label="Difficulty" rules={[{ required: true }]}>
            <Select>
              <Select.Option value="Easy">Easy</Select.Option>
              <Select.Option value="Medium">Medium</Select.Option>
              <Select.Option value="Hard">Hard</Select.Option>
            </Select>
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Add/Edit Vocab Modal */}
      <Modal
        title={editingVocab ? 'Edit Vocabulary' : 'Add Vocabulary'}
        open={isVocabModalOpen}
        onCancel={() => setIsVocabModalOpen(false)}
        onOk={() => vocabForm.submit()}
        okText="Save"
        confirmLoading={vocabLoading}
        width={420}
      >
        <Form form={vocabForm} layout="vertical" onFinish={handleSaveVocab} style={{ marginTop: 16 }}>
          <Form.Item name="word" label="Word" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="partOfSpeech" label="Part of Speech">
            <Select>
              {['Noun', 'Verb', 'Adjective', 'Adverb'].map(t => (
                <Select.Option key={t} value={t}>{t}</Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item name="pronunciation" label="Pronunciation"><Input placeholder="e.g. /ˈæpl/" /></Form.Item>
          <Form.Item name="meaning" label="Meaning" rules={[{ required: true }]}>
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item
            name="imageFile"
            label="Image"
            valuePropName="fileList"
            getValueFromEvent={(e: any) => Array.isArray(e) ? e : e?.fileList}
          >
            <Upload beforeUpload={() => false} maxCount={1} listType="picture">
              <Button size="small" icon={<UploadOutlined />}>Select Image</Button>
            </Upload>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
