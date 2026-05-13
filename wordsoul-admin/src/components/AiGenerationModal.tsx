'use client';

import React, { useState } from 'react';
import { Modal, Form, Input, Select, Button, Table, App, Tag, Row, Col, Steps } from 'antd';
import { RobotOutlined, ArrowRightOutlined, ArrowLeftOutlined, CheckOutlined } from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

interface AiGenerationModalProps {
  open: boolean;
  onCancel: () => void;
  onSuccess: () => void;
}

interface PreviewItem {
  word: string;
  meaning: string;
  pronunciation: string;
  partOfSpeech: string;
  exampleSentence?: string;
}

const STEP_ITEMS = [
  { title: 'Enter Words' },
  { title: 'AI Preview' },
  { title: 'Set Details' },
];

export default function AiGenerationModal({ open, onCancel, onSuccess }: AiGenerationModalProps) {
  const [step, setStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [previewData, setPreviewData] = useState<PreviewItem[]>([]);
  const [wordForm] = Form.useForm();
  const [configForm] = Form.useForm();
  const { message } = App.useApp();

  const handlePreview = async () => {
    try {
      const values = await wordForm.validateFields();
      const words = values.words
        .split('\n')
        .map((w: string) => w.trim())
        .filter((w: string) => w.length > 0);

      if (words.length === 0) {
        message.warning('Enter at least one word');
        return;
      }

      setLoading(true);
      const response = await authApi.post(endpoints.aiPreview, { words });
      setPreviewData(response.data);
      setStep(1);
    } catch (error: any) {
      message.error(error.response?.data || 'AI generation failed');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    try {
      const values = await configForm.validateFields();
      setLoading(true);

      const formData = new FormData();
      formData.append('Title', values.title);
      formData.append('Theme', values.theme);
      formData.append('DifficultyLevel', values.difficulty);
      formData.append('Description', values.description || '');
      formData.append('IsPublic', 'true');

      previewData.forEach((item, i) => {
        formData.append(`Vocabularies[${i}].Word`, item.word);
        formData.append(`Vocabularies[${i}].Meaning`, item.meaning);
        formData.append(`Vocabularies[${i}].Pronunciation`, item.pronunciation || '');
        formData.append(`Vocabularies[${i}].PartOfSpeech`, item.partOfSpeech || '');
      });

      await authApi.post(endpoints.aiCreate, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      message.success('Vocabulary Set created');
      handleClose();
      onSuccess();
    } catch (error: any) {
      message.error(error.response?.data?.Message || 'Failed to create set');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    wordForm.resetFields();
    configForm.resetFields();
    setPreviewData([]);
    setStep(0);
    onCancel();
  };

  const previewColumns = [
    {
      title: 'Word',
      dataIndex: 'word',
      key: 'word',
      render: (v: string) => <span style={{ fontWeight: 600, fontSize: 13 }}>{v}</span>,
    },
    {
      title: 'Type',
      dataIndex: 'partOfSpeech',
      key: 'partOfSpeech',
      width: 90,
      render: (v: string) => v ? <Tag style={{ fontSize: 11 }}>{v}</Tag> : null,
    },
    {
      title: 'Pronunciation',
      dataIndex: 'pronunciation',
      key: 'pronunciation',
      width: 130,
      render: (v: string) => <span style={{ fontSize: 12, color: 'var(--text-muted)', fontFamily: 'monospace' }}>{v}</span>,
    },
    {
      title: 'Meaning',
      dataIndex: 'meaning',
      key: 'meaning',
      render: (v: string) => <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{v}</span>,
    },
  ];

  return (
    <Modal
      title={
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <RobotOutlined style={{ color: 'var(--accent)', fontSize: 15 }} />
          <span style={{ fontSize: 14, fontWeight: 600 }}>AI Vocabulary Generation</span>
          <span style={{ fontSize: 11, color: 'var(--text-muted)', fontWeight: 400, marginLeft: 2 }}>
            Powered by Gemini
          </span>
        </div>
      }
      open={open}
      onCancel={handleClose}
      width={680}
      footer={null}
      destroyOnHidden
    >
      <Steps
        current={step}
        items={STEP_ITEMS}
        size="small"
        style={{ margin: '16px 0 24px' }}
      />

      {/* Step 0 — Word input */}
      {step === 0 && (
        <Form form={wordForm} layout="vertical">
          <Form.Item
            name="words"
            label={
              <span style={{ fontSize: 13 }}>
                Words to generate{' '}
                <span style={{ fontSize: 12, color: 'var(--text-muted)', fontWeight: 400 }}>
                  (one per line)
                </span>
              </span>
            }
            rules={[{ required: true, message: 'Please enter at least one word' }]}
          >
            <Input.TextArea
              rows={10}
              placeholder={'apple\nbanana\nambitious\nresign…'}
              style={{ fontFamily: 'monospace', fontSize: 13, resize: 'vertical' }}
            />
          </Form.Item>
          <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
            <Button
              type="primary"
              icon={<RobotOutlined />}
              iconPosition="start"
              onClick={handlePreview}
              loading={loading}
            >
              Generate with AI
            </Button>
          </div>
        </Form>
      )}

      {/* Step 1 — Preview */}
      {step === 1 && (
        <div>
          <div style={{ fontSize: 12, color: 'var(--text-muted)', marginBottom: 10 }}>
            {previewData.length} words generated. Review before proceeding.
          </div>
          <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden', marginBottom: 16 }}>
            <Table
              dataSource={previewData}
              columns={previewColumns}
              rowKey="word"
              pagination={false}
              size="small"
              scroll={{ y: 280 }}
            />
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <Button size="small" icon={<ArrowLeftOutlined />} onClick={() => setStep(0)}>
              Back
            </Button>
            <Button type="primary" size="small" icon={<ArrowRightOutlined />} iconPosition="end" onClick={() => setStep(2)}>
              Set Details
            </Button>
          </div>
        </div>
      )}

      {/* Step 2 — Config */}
      {step === 2 && (
        <Form form={configForm} layout="vertical">
          <Row gutter={12}>
            <Col span={14}>
              <Form.Item name="title" label="Set Title" rules={[{ required: true }]}>
                <Input placeholder="e.g. Common Travel Phrases" />
              </Form.Item>
            </Col>
            <Col span={10}>
              <Form.Item name="theme" label="Theme" rules={[{ required: true }]}>
                <Select placeholder="Select theme">
                  {['Academic', 'Casual', 'TOEIC', 'IELTS', 'Travel', 'Business'].map(t => (
                    <Select.Option key={t} value={t}>{t}</Select.Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="difficulty" label="Difficulty Level" rules={[{ required: true }]}>
            <Select placeholder="Select difficulty">
              <Select.Option value="Easy">Easy (A1–A2)</Select.Option>
              <Select.Option value="Medium">Medium (B1–B2)</Select.Option>
              <Select.Option value="Hard">Hard (C1–C2)</Select.Option>
            </Select>
          </Form.Item>
          <Form.Item name="description" label="Description (optional)">
            <Input.TextArea rows={2} placeholder="A short description about this vocabulary set…" />
          </Form.Item>
          <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid var(--border)', paddingTop: 16, marginTop: 4 }}>
            <Button size="small" icon={<ArrowLeftOutlined />} onClick={() => setStep(1)}>
              Back
            </Button>
            <Button
              type="primary"
              size="small"
              icon={<CheckOutlined />}
              onClick={handleCreate}
              loading={loading}
            >
              Create Vocabulary Set
            </Button>
          </div>
        </Form>
      )}
    </Modal>
  );
}
