'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import {
  Button, Table, Space, Tag, Popconfirm,
  Modal, Form, Input, Select, Upload, App, InputNumber,
} from 'antd';
import {
  ArrowLeftOutlined, EditOutlined, DeleteOutlined,
  PlusOutlined, UploadOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import dayjs from 'dayjs';
import ImageUploader from '../../_components/ImageUploader';

interface RewardPet {
  petId: number;
  petName?: string;
  petImageUrl?: string;
  rarity?: string;
  dropRate: number;
}

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
  const [editSetFile, setEditSetFile] = useState<File | null>(null);

  // Reward pets
  const [rewardPets, setRewardPets] = useState<RewardPet[]>([]);
  const [petModalOpen, setPetModalOpen] = useState(false);
  const [petForm] = Form.useForm();
  const [petSubmitting, setPetSubmitting] = useState(false);
  const [editingPet, setEditingPet] = useState<RewardPet | null>(null);

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

  const fetchRewardPets = useCallback(async () => {
    try {
      const { data: pets } = await authApi.get(endpoints.vocabularySetRewardPets(Number(id)));
      setRewardPets(Array.isArray(pets) ? pets : []);
    } catch { /* silent */ }
  }, [id]);

  useEffect(() => { if (id) { fetchDetails(); fetchRewardPets(); } }, [id]);

  const handleUpdateSet = async (values: any) => {
    try {
      const fd = new FormData();
      fd.append('title', values.title);
      fd.append('theme', values.theme);
      fd.append('description', values.description ?? '');
      fd.append('difficultyLevel', values.difficultyLevel);
      fd.append('isActive', String(values.isActive ?? true));
      fd.append('vocabularyIds', '[]');
      if (editSetFile) fd.append('imageFile', editSetFile);
      await authApi.put(`${endpoints.vocabularySets}/${id}`, fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      message.success('Set updated');
      setIsEditSetModalOpen(false);
      setEditSetFile(null);
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

  const openAddPetModal = () => {
    setEditingPet(null);
    petForm.resetFields();
    setPetModalOpen(true);
  };

  const openEditPetModal = (pet: RewardPet) => {
    setEditingPet(pet);
    petForm.setFieldsValue({ petId: pet.petId, dropRate: pet.dropRate });
    setPetModalOpen(true);
  };

  const handlePetSubmit = async (values: any) => {
    setPetSubmitting(true);
    try {
      if (editingPet) {
        await authApi.put(endpoints.vocabularySetRewardPet(Number(id), editingPet.petId), { petId: values.petId, dropRate: values.dropRate });
        message.success('Drop rate updated');
      } else {
        await authApi.post(endpoints.vocabularySetRewardPets(Number(id)), { petId: values.petId, dropRate: values.dropRate });
        message.success('Reward pet added');
      }
      setPetModalOpen(false);
      fetchRewardPets();
    } catch { message.error('Failed to save reward pet'); }
    finally { setPetSubmitting(false); }
  };

  const handleRemovePet = async (petId: number) => {
    try {
      await authApi.delete(endpoints.vocabularySetRewardPet(Number(id), petId));
      message.success('Reward pet removed');
      fetchRewardPets();
    } catch { message.error('Failed to remove reward pet'); }
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
                onClick={() => {
                  form.setFieldsValue({
                    title: data.title,
                    description: data.description,
                    theme: data.theme,
                    difficultyLevel: data.difficultyLevel?.split(' ')[0] ?? 'Easy',
                    isActive: data.isActive,
                  });
                  setEditSetFile(null);
                  setIsEditSetModalOpen(true);
                }}
              >
                Edit Set Info
              </Button>
            </div>
          </div>
        </div>

        {/* Right: Words table + Reward Pets */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          {/* Words */}
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

          {/* Reward Pets */}
          <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
            <div style={{ padding: '12px 16px', borderBottom: '1px solid var(--border)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>
                Reward Pets
                <span style={{ fontSize: 12, fontWeight: 400, color: 'var(--text-muted)', marginLeft: 6 }}>
                  ({rewardPets.length})
                </span>
              </span>
              <Button size="small" icon={<PlusOutlined />} onClick={openAddPetModal}>Add Pet</Button>
            </div>
            <Table
              dataSource={rewardPets}
              rowKey="petId"
              size="small"
              pagination={false}
              columns={[
                {
                  title: 'Image',
                  dataIndex: 'petImageUrl',
                  width: 52,
                  render: (url?: string) => url
                    ? <img src={url} alt="" style={{ width: 32, height: 32, borderRadius: 6, objectFit: 'cover', border: '1px solid var(--border)' }} />
                    : <div style={{ width: 32, height: 32, borderRadius: 6, background: 'var(--bg-muted)', border: '1px solid var(--border)' }} />,
                },
                { title: 'Pet Name', dataIndex: 'petName', render: (v?: string) => <span style={{ fontWeight: 500, fontSize: 13 }}>{v ?? `Pet #${v}`}</span> },
                { title: 'Rarity', dataIndex: 'rarity', width: 90, render: (v?: string) => v ? <Tag style={{ fontSize: 11 }}>{v}</Tag> : null },
                {
                  title: 'Drop Rate',
                  dataIndex: 'dropRate',
                  width: 100,
                  render: (v: number) => <span style={{ fontSize: 12, fontWeight: 600, fontVariantNumeric: 'tabular-nums' }}>{(v * 100).toFixed(1)}%</span>,
                },
                {
                  title: '',
                  key: 'actions',
                  width: 80,
                  render: (_: any, record: RewardPet) => (
                    <Space size={4}>
                      <Button size="small" icon={<EditOutlined />} onClick={() => openEditPetModal(record)} />
                      <Popconfirm title="Remove pet?" okText="Remove" okButtonProps={{ danger: true }} onConfirm={() => handleRemovePet(record.petId)}>
                        <Button size="small" danger icon={<DeleteOutlined />} />
                      </Popconfirm>
                    </Space>
                  ),
                },
              ]}
            />
          </div>
        </div>
      </div>

      {/* Edit Set Modal */}
      <Modal
        title="Edit Vocabulary Set"
        open={isEditSetModalOpen}
        onCancel={() => { setIsEditSetModalOpen(false); setEditSetFile(null); }}
        onOk={() => form.submit()}
        okText="Save"
        width={460}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleUpdateSet} style={{ marginTop: 16 }}>
          <Form.Item label="Cover Image" style={{ marginBottom: 12 }}>
            <ImageUploader value={data?.imageUrl} onChange={setEditSetFile} />
          </Form.Item>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="theme" label="Theme" rules={[{ required: true }]}>
            <Select>
              {['DailyLife','Nature','Food','Weather','Technology','Travel','Health','Sports','Business','Science','Art','Communication','Mystery','Dark','Academic','Challenge','TrapWords','System','Custom'].map(t => (
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
      {/* Add/Edit Reward Pet Modal */}
      <Modal
        title={editingPet ? 'Edit Drop Rate' : 'Add Reward Pet'}
        open={petModalOpen}
        onOk={() => petForm.submit()}
        onCancel={() => { setPetModalOpen(false); petForm.resetFields(); }}
        confirmLoading={petSubmitting}
        okText={editingPet ? 'Save' : 'Add'}
        width={340}
        destroyOnHidden
      >
        <Form form={petForm} layout="vertical" onFinish={handlePetSubmit} style={{ marginTop: 12 }}>
          <Form.Item name="petId" label="Pet ID" rules={[{ required: true, message: 'Required' }]}>
            <InputNumber style={{ width: '100%' }} min={1} disabled={!!editingPet} />
          </Form.Item>
          <Form.Item name="dropRate" label="Drop Rate (0.0 – 1.0)" rules={[{ required: true }]} initialValue={0.1}>
            <InputNumber style={{ width: '100%' }} min={0.01} max={1} step={0.05} precision={2} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
