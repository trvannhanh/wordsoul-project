'use client';

import React, { useState } from 'react';
import {
  App, Button, Card, Col, Form, Input, Popconfirm, Radio,
  Row, Select, Statistic, Tag, Typography,
} from 'antd';
import {
  BellOutlined, SendOutlined, UserOutlined, TeamOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

const { Text, Paragraph } = Typography;

const TYPE_OPTIONS = [
  { label: 'Review',  value: 'Review',  color: '#0369a1' },
  { label: 'Reward',  value: 'Reward',  color: '#d97706' },
  { label: 'Event',   value: 'Event',   color: '#7c3aed' },
];

const TYPE_COLOR: Record<string, string> = {
  Review: '#0369a1',
  Reward: '#d97706',
  Event:  '#7c3aed',
};

interface BroadcastResult {
  notificationsSent: number;
  broadcastAt: string;
}

export default function NotificationsPage() {
  const { message } = App.useApp();
  const [form] = Form.useForm();
  const [sending, setSending] = useState(false);
  const [lastResult, setLastResult] = useState<BroadcastResult | null>(null);

  // Live preview values
  const title   = Form.useWatch('title',    form) ?? '';
  const msg     = Form.useWatch('message',  form) ?? '';
  const type    = Form.useWatch('type',     form) ?? 'Review';
  const audience = Form.useWatch('audience', form) ?? 'all';
  const targetIds: string[] = Form.useWatch('targetIds', form) ?? [];

  const handleSend = async () => {
    try {
      const values = await form.validateFields();
      setSending(true);

      const payload = {
        title:         values.title,
        message:       values.message,
        type:          values.type,
        targetUserIds: values.audience === 'specific'
          ? (values.targetIds as string[]).map(Number).filter((n) => !isNaN(n))
          : null,
      };

      const res = await authApi.post(endpoints.notificationBroadcast, payload);
      const result: BroadcastResult = res.data;
      setLastResult(result);
      message.success(`Sent to ${result.notificationsSent} user(s)`);
      form.resetFields();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'errorFields' in err) return;
      message.error('Failed to send broadcast');
    } finally {
      setSending(false);
    }
  };

  const audienceLabel = audience === 'all'
    ? 'All Users'
    : targetIds.length > 0
      ? `${targetIds.length} specific user(s)`
      : 'No users selected';

  return (
    <div>
      {/* ── Page header ── */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Notification Broadcast</h1>
          <p className="page-subtitle">Send system notifications to all users or a specific group.</p>
        </div>
      </div>

      <Row gutter={24} align="top">
        {/* ── Compose form ── */}
        <Col xs={24} lg={14}>
          <Card
            title={
              <span>
                <BellOutlined style={{ marginRight: 8, color: 'var(--accent)' }} />
                Compose Notification
              </span>
            }
            style={{ borderRadius: 10 }}
          >
            <Form
              form={form}
              layout="vertical"
              initialValues={{ type: 'Review', audience: 'all' }}
            >
              <Form.Item
                name="title"
                label="Title"
                rules={[
                  { required: true, message: 'Title is required' },
                  { max: 100, message: 'Max 100 characters' },
                ]}
              >
                <Input maxLength={100} showCount placeholder="e.g. New event available!" />
              </Form.Item>

              <Form.Item
                name="message"
                label="Message"
                rules={[
                  { required: true, message: 'Message is required' },
                  { max: 200, message: 'Max 200 characters' },
                ]}
              >
                <Input.TextArea
                  maxLength={200}
                  showCount
                  rows={3}
                  placeholder="Brief message body shown in the notification..."
                />
              </Form.Item>

              <Row gutter={16}>
                <Col span={12}>
                  <Form.Item name="type" label="Type">
                    <Select options={TYPE_OPTIONS} />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item name="audience" label="Audience">
                    <Radio.Group>
                      <Radio.Button value="all">
                        <TeamOutlined style={{ marginRight: 4 }} />
                        All Users
                      </Radio.Button>
                      <Radio.Button value="specific">
                        <UserOutlined style={{ marginRight: 4 }} />
                        Specific
                      </Radio.Button>
                    </Radio.Group>
                  </Form.Item>
                </Col>
              </Row>

              {audience === 'specific' && (
                <Form.Item
                  name="targetIds"
                  label="User IDs"
                  rules={[{ required: true, message: 'Enter at least one User ID' }]}
                  extra="Type a numeric user ID and press Enter to add."
                >
                  <Select
                    mode="tags"
                    tokenSeparators={[',', ' ']}
                    placeholder="e.g. 1, 42, 100"
                    style={{ width: '100%' }}
                  />
                </Form.Item>
              )}

              <Form.Item style={{ marginBottom: 0, marginTop: 8 }}>
                <Popconfirm
                  title="Send notification?"
                  description={`This will notify ${audienceLabel}. Proceed?`}
                  onConfirm={handleSend}
                  okText="Send"
                  cancelText="Cancel"
                  okButtonProps={{ loading: sending }}
                >
                  <Button
                    type="primary"
                    icon={<SendOutlined />}
                    loading={sending}
                    block
                  >
                    Send Broadcast
                  </Button>
                </Popconfirm>
              </Form.Item>
            </Form>
          </Card>

          {/* ── Last result ── */}
          {lastResult && (
            <Card
              style={{ marginTop: 16, borderRadius: 10, borderColor: 'var(--success)' }}
              size="small"
            >
              <Row gutter={16}>
                <Col span={12}>
                  <Statistic
                    title="Notifications Sent"
                    value={lastResult.notificationsSent}
                    prefix={<BellOutlined />}
                    styles={{ content: { color: 'var(--success)', fontWeight: 700 } }}
                  />
                </Col>
                <Col span={12}>
                  <Statistic
                    title="Broadcast At"
                    value={new Date(lastResult.broadcastAt).toLocaleString()}
                    styles={{ content: { fontSize: 13 } }}
                  />
                </Col>
              </Row>
            </Card>
          )}
        </Col>

        {/* ── Live preview ── */}
        <Col xs={24} lg={10}>
          <Card
            title="Live Preview"
            style={{ borderRadius: 10, position: 'sticky', top: 24 }}
            size="small"
          >
            <div
              style={{
                background: 'var(--bg-base)',
                border: '1px solid var(--border)',
                borderRadius: 10,
                padding: '14px 16px',
                display: 'flex',
                gap: 12,
                alignItems: 'flex-start',
              }}
            >
              {/* Bell icon bubble */}
              <div
                style={{
                  width: 40,
                  height: 40,
                  borderRadius: 20,
                  background: `${TYPE_COLOR[type]}22`,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  flexShrink: 0,
                }}
              >
                <BellOutlined style={{ fontSize: 18, color: TYPE_COLOR[type] }} />
              </div>

              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 4 }}>
                  <Text
                    strong
                    style={{
                      fontSize: 13,
                      color: 'var(--text-primary)',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {title || <span style={{ color: 'var(--text-muted)', fontStyle: 'italic' }}>Title goes here</span>}
                  </Text>
                  <Tag
                    color={TYPE_COLOR[type]}
                    style={{ fontSize: 10, lineHeight: '16px', padding: '0 5px', flexShrink: 0 }}
                  >
                    {type}
                  </Tag>
                </div>
                <Paragraph
                  style={{
                    fontSize: 12,
                    color: 'var(--text-muted)',
                    margin: 0,
                    lineHeight: '1.5',
                  }}
                  ellipsis={{ rows: 2 }}
                >
                  {msg || <span style={{ fontStyle: 'italic' }}>Message goes here...</span>}
                </Paragraph>
                <div style={{ fontSize: 10, color: 'var(--text-muted)', marginTop: 6 }}>
                  Just now · {audienceLabel}
                </div>
              </div>
            </div>

            <div style={{ marginTop: 16 }}>
              <Text type="secondary" style={{ fontSize: 11 }}>
                Notification type:&nbsp;
                <Tag color={TYPE_COLOR[type]} style={{ fontSize: 11 }}>{type}</Tag>
              </Text>
              <br />
              <Text type="secondary" style={{ fontSize: 11 }}>
                Recipients:&nbsp;
                <strong>{audienceLabel}</strong>
              </Text>
            </div>
          </Card>
        </Col>
      </Row>
    </div>
  );
}
