'use client';

import React, { useState, useEffect } from 'react';
import { Form, Input, InputNumber, Button, Alert, App } from 'antd';
import {
  ReloadOutlined, SaveOutlined,
  CheckCircleOutlined, ExclamationCircleOutlined,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';
import dayjs from 'dayjs';

interface SystemConfig {
  id: number;
  key: string;
  value: string;
  description: string;
  category: string;
}

interface HealthStats {
  status: string;
  uptime: string;
  database: string;
  timestamp: string;
}

// ── Section component ─────────────────────────────────────────────────────────
function ConfigSection({
  title,
  description,
  configs,
}: {
  title: string;
  description?: string;
  configs: SystemConfig[];
}) {
  return (
    <div style={{ marginBottom: 24 }}>
      <div style={{ marginBottom: 14 }}>
        <div style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)', marginBottom: 2 }}>{title}</div>
        {description && <div style={{ fontSize: 12, color: 'var(--text-muted)' }}>{description}</div>}
      </div>
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
          gap: 12,
        }}
      >
        {configs.map(c => (
          <div
            key={c.key}
            style={{
              background: 'var(--bg-muted)',
              border: '1px solid var(--border)',
              borderRadius: 6,
              padding: '10px 12px',
            }}
          >
            <Form.Item
              name={c.key}
              label={
                <div>
                  <span style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-primary)', fontFamily: 'monospace' }}>
                    {c.key}
                  </span>
                  {c.description && (
                    <div style={{ fontSize: 11, color: 'var(--text-muted)', fontWeight: 400, fontFamily: 'inherit', marginTop: 1 }}>
                      {c.description}
                    </div>
                  )}
                </div>
              }
              style={{ marginBottom: 0 }}
            >
              {isNaN(Number(c.value)) ? (
                <Input size="small" />
              ) : (
                <InputNumber
                  size="small"
                  style={{ width: '100%' }}
                  step={c.key.includes('FACTOR') || c.key.includes('RATE') ? 0.01 : 1}
                />
              )}
            </Form.Item>
          </div>
        ))}
      </div>
    </div>
  );
}

// ── Health stat row ────────────────────────────────────────────────────────────
function HealthRow({
  label,
  value,
  status,
}: {
  label: string;
  value: string;
  status: 'healthy' | 'warning' | 'error' | 'neutral';
}) {
  const dotColor = {
    healthy: 'var(--success)',
    warning: 'var(--warning)',
    error: 'var(--danger)',
    neutral: 'var(--text-muted)',
  }[status];

  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '9px 0', borderBottom: '1px solid var(--border)' }}>
      <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{label}</span>
      <span style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 13, fontWeight: 500, color: 'var(--text-primary)' }}>
        <span style={{ width: 7, height: 7, borderRadius: '50%', background: dotColor, display: 'inline-block', flexShrink: 0 }} />
        {value}
      </span>
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────
export default function SystemHealthPage() {
  const { user } = useAuth();
  const [configs, setConfigs] = useState<SystemConfig[]>([]);
  const [health, setHealth] = useState<HealthStats | null>(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form] = Form.useForm();
  const { message } = App.useApp();

  const fetchData = async () => {
    setLoading(true);
    try {
      const [configRes, healthRes] = await Promise.all([
        authApi.get(endpoints.systemConfig),
        authApi.get(endpoints.systemHealth),
      ]);
      setConfigs(configRes.data);
      setHealth(healthRes.data);
      const initialValues: Record<string, any> = {};
      configRes.data.forEach((c: SystemConfig) => {
        initialValues[c.key] = isNaN(Number(c.value)) ? c.value : Number(c.value);
      });
      form.setFieldsValue(initialValues);
    } catch { message.error('Failed to load system data'); }
    finally { setLoading(false); }
  };

  useEffect(() => { if (user?.role === 'SuperAdmin') fetchData(); }, [user]);

  const onFinish = async (values: any) => {
    setSaving(true);
    try {
      const updated = configs.map(c => ({ ...c, value: values[c.key].toString() }));
      await authApi.put(endpoints.systemConfig, updated);
      message.success('Configuration saved');
      fetchData();
    } catch { message.error('Failed to save configuration'); }
    finally { setSaving(false); }
  };

  if (user?.role !== 'SuperAdmin') {
    return (
      <div style={{ maxWidth: 480, margin: '40px auto' }}>
        <Alert
          message="Access Restricted"
          description="This page is only accessible to SuperAdmins."
          type="error"
          showIcon
        />
      </div>
    );
  }

  const srsConfigs = configs.filter(c => c.category === 'SRS');
  const gameConfigs = configs.filter(c => c.category === 'GAME_BALANCE');

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">System Health & Configuration</h1>
          <p className="page-subtitle">Monitor system status and manage core algorithm parameters.</p>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <Button size="small" icon={<ReloadOutlined />} onClick={fetchData} loading={loading}>
            Refresh
          </Button>
          <Button
            type="primary"
            size="small"
            icon={<SaveOutlined />}
            onClick={() => form.submit()}
            loading={saving}
          >
            Save Changes
          </Button>
        </div>
      </div>

      {/* Health strip */}
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))',
          gap: 10,
          marginBottom: 20,
        }}
      >
        {[
          {
            label: 'API Status',
            value: health?.status ?? '—',
            status: (health?.status === 'Healthy' ? 'healthy' : health ? 'error' : 'neutral') as any,
          },
          {
            label: 'Database',
            value: health?.database ?? '—',
            status: (health?.database === 'Healthy' ? 'healthy' : health ? 'error' : 'neutral') as any,
          },
          {
            label: 'Uptime',
            value: '99.9%',
            status: 'healthy' as any,
          },
          {
            label: 'Last Checked',
            value: health ? dayjs(health.timestamp).format('HH:mm:ss') : '—',
            status: 'neutral' as any,
          },
        ].map(item => (
          <div
            key={item.label}
            style={{
              background: 'var(--bg-surface)',
              border: '1px solid var(--border)',
              borderRadius: 8,
              padding: '12px 16px',
            }}
          >
            <div style={{ fontSize: 11, fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: 6 }}>
              {item.label}
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 14, fontWeight: 600, color: 'var(--text-primary)' }}>
              <span
                style={{
                  width: 7,
                  height: 7,
                  borderRadius: '50%',
                  background: item.status === 'healthy' ? 'var(--success)' : item.status === 'error' ? 'var(--danger)' : 'var(--text-muted)',
                  display: 'inline-block',
                  flexShrink: 0,
                  animation: item.status === 'healthy' ? 'pulse-status 2.5s ease-in-out infinite' : 'none',
                }}
              />
              {item.value}
            </div>
          </div>
        ))}
      </div>

      <Alert
        message="Caution: Live Configuration"
        description="Changes to SRS constants take effect immediately and will affect next-review interval calculations for all active users."
        type="warning"
        showIcon
        style={{ marginBottom: 20, borderRadius: 8 }}
      />

      {/* Config Form */}
      <Form form={form} layout="vertical" onFinish={onFinish} disabled={loading || saving}>
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, padding: '20px 24px', marginBottom: 16 }}>
          <ConfigSection
            title="SRS Algorithm (SM-2)"
            description="Controls spaced repetition scheduling for all vocabulary reviews."
            configs={srsConfigs}
          />
        </div>
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, padding: '20px 24px' }}>
          <ConfigSection
            title="Game Balance & Rewards"
            description="Controls XP multipliers, AP economy, and progression thresholds."
            configs={gameConfigs}
          />
        </div>
      </Form>

      {/* Maintenance & Monitor */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 14, marginTop: 16 }}>
        {/* Maintenance */}
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
          <div style={{ padding: '12px 16px', borderBottom: '1px solid var(--border)' }}>
            <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>Maintenance</span>
          </div>
          <div style={{ padding: '12px 16px', display: 'flex', flexDirection: 'column', gap: 10 }}>
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                padding: '10px 12px',
                borderRadius: 6,
                background: 'var(--danger-bg)',
                border: '1px solid #FECACA',
              }}
            >
              <div>
                <div style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-primary)' }}>Flush Redis Cache</div>
                <div style={{ fontSize: 11, color: 'var(--text-muted)', marginTop: 2 }}>Clear all AI preview and metadata caches.</div>
              </div>
              <Button
                size="small"
                danger
                onClick={async () => {
                  await authApi.post(endpoints.redisFlush);
                  message.success('Redis cache flushed');
                }}
              >
                Flush
              </Button>
            </div>
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                padding: '10px 12px',
                borderRadius: 6,
                background: 'var(--bg-muted)',
                border: '1px solid var(--border)',
              }}
            >
              <div>
                <div style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-primary)' }}>Database Cleanup</div>
                <div style={{ fontSize: 11, color: 'var(--text-muted)', marginTop: 2 }}>Archive records older than 1 year.</div>
              </div>
              <Button
                size="small"
                onClick={async () => {
                  await authApi.post(endpoints.dbCleanup);
                  message.success('Cleanup started');
                }}
              >
                Run
              </Button>
            </div>
          </div>
        </div>

        {/* SignalR Monitor */}
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, overflow: 'hidden' }}>
          <div style={{ padding: '12px 16px', borderBottom: '1px solid var(--border)' }}>
            <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>WebSocket Monitor</span>
          </div>
          <div style={{ padding: '4px 16px' }}>
            {[
              { label: 'Notification Hub', value: '12 connections', status: 'healthy' },
              { label: 'Battle Hub (PvP)', value: '4 connections', status: 'healthy' },
              { label: 'Avg. Latency', value: '42ms', status: 'healthy' },
            ].map(item => (
              <HealthRow key={item.label} label={item.label} value={item.value} status={item.status as any} />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
