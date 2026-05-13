'use client';

import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Input, InputNumber, App, Row, Col } from 'antd';
import { EditOutlined } from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

export default function GymsPage() {
  const [gyms, setGyms] = useState([]);
  const [loading, setLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingGym, setEditingGym] = useState<any>(null);
  const [form] = Form.useForm();
  const { message } = App.useApp();

  const fetchGyms = async () => {
    setLoading(true);
    try {
      const res = await authApi.get(endpoints.adminGyms);
      setGyms(res.data);
    } catch { message.error('Failed to fetch gyms'); }
    finally { setLoading(false); }
  };

  useEffect(() => { fetchGyms(); }, []);

  const handleUpdate = async (values: any) => {
    try {
      await authApi.put(`${endpoints.adminGyms}/${editingGym.id}`, values);
      message.success('Gym updated');
      setIsModalOpen(false);
      fetchGyms();
    } catch { message.error('Failed to update gym'); }
  };

  const openEdit = (record: any) => {
    setEditingGym(record);
    form.setFieldsValue({
      ...record,
      aiReactionTimeMs: record.gymLeaderPets?.[0]?.botAvgResponseMs ?? 2000,
    });
    setIsModalOpen(true);
  };

  const columns = [
    {
      title: 'Badge',
      dataIndex: 'badgeImageUrl',
      key: 'badge',
      width: 56,
      render: (url: string) => url
        ? <img src={url} alt="" style={{ width: 36, height: 36, objectFit: 'contain', borderRadius: 4, border: '1px solid var(--border)' }} />
        : <div style={{ width: 36, height: 36, borderRadius: 4, background: 'var(--bg-muted)', border: '1px solid var(--border)' }} />,
    },
    {
      title: 'Gym Name',
      dataIndex: 'name',
      key: 'name',
      render: (v: string) => <span style={{ fontWeight: 500, fontSize: 13 }}>{v}</span>,
    },
    {
      title: 'Required XP',
      dataIndex: 'xpThreshold',
      key: 'xpThreshold',
      width: 110,
      render: (v: number) => <span style={{ fontSize: 12, fontVariantNumeric: 'tabular-nums' }}>{v?.toLocaleString()}</span>,
    },
    {
      title: 'Pass Rate',
      dataIndex: 'passRatePercent',
      key: 'passRatePercent',
      width: 90,
      render: (v: number) => (
        <span style={{
          fontSize: 12,
          fontWeight: 600,
          color: v >= 70 ? 'var(--success)' : v >= 40 ? 'var(--warning)' : 'var(--danger)',
          fontVariantNumeric: 'tabular-nums',
        }}>
          {v}%
        </span>
      ),
    },
    {
      title: 'AI Reaction Time',
      dataIndex: 'aiReactionTimeMs',
      key: 'aiReactionTimeMs',
      width: 130,
      render: (v: number) => (
        <span style={{ fontSize: 12, color: 'var(--text-muted)', fontVariantNumeric: 'tabular-nums' }}>
          {v}ms
        </span>
      ),
    },
    {
      title: '',
      key: 'action',
      width: 60,
      render: (_: any, record: any) => (
        <Button
          type="text"
          size="small"
          icon={<EditOutlined />}
          onClick={() => openEdit(record)}
          style={{ color: 'var(--text-muted)' }}
        />
      ),
    },
  ];

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Gym Operations</h1>
          <p className="page-subtitle">Configure gym difficulty thresholds, pass rates, and AI reaction speed.</p>
        </div>
      </div>

      {/* Table */}
      <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
        <Table
          dataSource={gyms}
          columns={columns}
          rowKey="id"
          loading={loading}
          size="small"
          pagination={false}
          expandable={{
            expandedRowRender: (record: any) => (
              <div style={{ padding: '12px 16px', background: 'var(--bg-muted)', borderTop: '1px solid var(--border)' }}>
                <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: 10 }}>
                  Boss Lineup
                </div>
                <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
                  {record.gymLeaderPets?.map((gp: any) => (
                    <div
                      key={gp.id}
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 8,
                        background: 'var(--bg-surface)',
                        border: '1px solid var(--border)',
                        borderRadius: 6,
                        padding: '6px 10px',
                      }}
                    >
                      {gp.pet?.imageUrl && (
                        <img src={gp.pet.imageUrl} alt="" style={{ width: 28, height: 28, borderRadius: 4, objectFit: 'contain' }} />
                      )}
                      <div>
                        <div style={{ fontSize: 12, fontWeight: 500, color: 'var(--text-primary)' }}>{gp.pet?.name}</div>
                        <div style={{ fontSize: 11, color: 'var(--text-muted)' }}>Lvl {gp.level}</div>
                      </div>
                    </div>
                  ))}
                  {!record.gymLeaderPets?.length && (
                    <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>No pets configured.</span>
                  )}
                </div>
              </div>
            ),
            rowExpandable: (record: any) => record.gymLeaderPets?.length > 0,
          }}
        />
      </div>

      {/* Edit Modal */}
      <Modal
        title={`Edit — ${editingGym?.name}`}
        open={isModalOpen}
        onCancel={() => setIsModalOpen(false)}
        onOk={() => form.submit()}
        okText="Save"
        width={400}
      >
        <Form form={form} layout="vertical" onFinish={handleUpdate} style={{ marginTop: 16 }}>
          <Form.Item name="name" label="Gym Name" rules={[{ required: true }]}><Input /></Form.Item>
          <Form.Item name="description" label="Description"><Input.TextArea rows={2} /></Form.Item>
          <Row gutter={12}>
            <Col span={12}>
              <Form.Item name="xpThreshold" label="Required XP">
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="passRatePercent" label="Pass Rate (%)">
                <InputNumber style={{ width: '100%' }} min={0} max={100} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item
            name="aiReactionTimeMs"
            label="AI Reaction Time (ms)"
            extra="Lower = faster AI response. Minimum 500ms."
          >
            <InputNumber style={{ width: '100%' }} min={500} step={100} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
