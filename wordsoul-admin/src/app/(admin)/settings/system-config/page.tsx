'use client';

import React, { useState, useEffect } from 'react';
import { Form, Input, InputNumber, Button, Alert, App } from 'antd';
import { ReloadOutlined, SaveOutlined } from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

interface SystemConfig {
  key: string;
  value: string;
  dataType: string;
  description: string;
  category: string;
  lastUpdatedAt: string;
  lastUpdatedBy?: string;
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

// ── Page ──────────────────────────────────────────────────────────────────────
export default function SystemConfigPage() {
  const { user } = useAuth();
  const [configs, setConfigs] = useState<SystemConfig[]>([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form] = Form.useForm();
  const { message } = App.useApp();

  const fetchConfigs = async () => {
    setLoading(true);
    try {
      const { data } = await authApi.get<SystemConfig[]>(endpoints.systemConfig);
      setConfigs(data);
      const initialValues: Record<string, unknown> = {};
      data.forEach(c => {
        initialValues[c.key] = isNaN(Number(c.value)) ? c.value : Number(c.value);
      });
      form.setFieldsValue(initialValues);
    } catch {
      message.error('Failed to load configurations');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { if (user?.role === 'SuperAdmin') fetchConfigs(); }, [user]);

  const onFinish = async (values: Record<string, unknown>) => {
    setSaving(true);
    try {
      const updated = configs.map(c => ({ ...c, value: String(values[c.key]) }));
      await authApi.put(endpoints.systemConfig, updated);
      message.success('Configuration saved');
      fetchConfigs();
    } catch {
      message.error('Failed to save configuration');
    } finally {
      setSaving(false);
    }
  };

  if (user?.role !== 'SuperAdmin') {
    return (
      <div style={{ maxWidth: 480, margin: '40px auto' }}>
        <Alert
          title="Access Restricted"
          description="This page is only accessible to SuperAdmins."
          type="error"
          showIcon
        />
      </div>
    );
  }

  const srsConfigs = configs.filter(c => c.category === 'SRS');
  const gameConfigs = configs.filter(c => c.category === 'GAME_BALANCE');
  const systemConfigs = configs.filter(c => c.category === 'SYSTEM');

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">System Configuration</h1>
          <p className="page-subtitle">Manage core algorithm parameters and game balance constants.</p>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <Button size="small" icon={<ReloadOutlined />} onClick={fetchConfigs} loading={loading}>
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

      <Alert
        title="Caution: Live Configuration"
        description="Changes to SRS constants take effect immediately and will affect next-review interval calculations for all active users."
        type="warning"
        showIcon
        style={{ marginBottom: 20, borderRadius: 8 }}
      />

      <Form form={form} layout="vertical" onFinish={onFinish} disabled={loading || saving}>
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, padding: '20px 24px', marginBottom: 16 }}>
          <ConfigSection
            title="SRS Algorithm (SM-2)"
            description="Controls spaced repetition scheduling for all vocabulary reviews."
            configs={srsConfigs}
          />
        </div>
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, padding: '20px 24px', marginBottom: 16 }}>
          <ConfigSection
            title="Game Balance & Rewards"
            description="Controls XP multipliers, AP economy, and progression thresholds."
            configs={gameConfigs}
          />
        </div>
        <div style={{ background: 'var(--bg-surface)', border: '1px solid var(--border)', borderRadius: 8, padding: '20px 24px' }}>
          <ConfigSection
            title="System Settings"
            description="Controls background jobs, logs retention, and system-level operations."
            configs={systemConfigs}
          />
        </div>
      </Form>
    </div>
  );
}
