'use client';

import React, { useState, useEffect } from 'react';
import {
  Form, Switch, InputNumber, Input, Button, Alert, App,
  Typography, Divider,
} from 'antd';
import { ReloadOutlined, SaveOutlined } from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

const { Text } = Typography;

interface SystemConfig {
  key: string;
  value: string;
  dataType: string;
  description: string;
  category: string;
  lastUpdatedAt: string;
  lastUpdatedBy?: string;
}

// ─── Labels / descriptions override ──────────────────────────────────────────
const KEY_META: Record<string, { label: string; description: string }> = {
  AllowRegistration: {
    label: 'Allow New Registrations',
    description: 'When disabled, the registration page is locked and new users cannot create accounts.',
  },
  MaintenanceMode: {
    label: 'Maintenance Mode',
    description: 'Redirects regular users to a maintenance page. Admins and SuperAdmins are unaffected.',
  },
  MaxGroupSize: {
    label: 'Max Group Size',
    description: 'Maximum number of members allowed in a single user group.',
  },
  AppDisplayName: {
    label: 'App Display Name',
    description: 'Name shown to users throughout the UI (e.g. in the browser tab and welcome message).',
  },
};

// ─── Single setting row ───────────────────────────────────────────────────────
function SettingRow({ config }: { config: SystemConfig }) {
  const meta = KEY_META[config.key];
  const label = meta?.label ?? config.key;
  const description = meta?.description ?? config.description;

  const control = (() => {
    if (config.dataType === 'Boolean') {
      return (
        <Form.Item name={config.key} valuePropName="checked" style={{ marginBottom: 0 }}>
          <Switch />
        </Form.Item>
      );
    }
    if (config.dataType === 'Integer' || config.dataType === 'Float') {
      return (
        <Form.Item name={config.key} style={{ marginBottom: 0 }}>
          <InputNumber
            size="small"
            style={{ width: 120 }}
            step={config.dataType === 'Float' ? 0.01 : 1}
          />
        </Form.Item>
      );
    }
    return (
      <Form.Item name={config.key} style={{ marginBottom: 0 }}>
        <Input size="small" style={{ width: 240 }} />
      </Form.Item>
    );
  })();

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: '14px 20px',
        borderBottom: '1px solid var(--border)',
      }}
    >
      <div style={{ flex: 1, marginRight: 24 }}>
        <Text style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)', display: 'block' }}>
          {label}
        </Text>
        {description && (
          <Text type="secondary" style={{ fontSize: 12 }}>
            {description}
          </Text>
        )}
      </div>
      {control}
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────
export default function GeneralsPage() {
  const { user } = useAuth();
  const [allConfigs, setAllConfigs] = useState<SystemConfig[]>([]);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form] = Form.useForm();
  const { message } = App.useApp();

  const fetchConfigs = async () => {
    setLoading(true);
    try {
      const { data } = await authApi.get<SystemConfig[]>(endpoints.systemConfig);
      setAllConfigs(data);
      const initialValues: Record<string, unknown> = {};
      data
        .filter(c => c.category === 'GENERAL')
        .forEach(c => {
          if (c.dataType === 'Boolean') {
            initialValues[c.key] = c.value === 'true';
          } else if (c.dataType === 'Integer') {
            initialValues[c.key] = parseInt(c.value, 10);
          } else if (c.dataType === 'Float') {
            initialValues[c.key] = parseFloat(c.value);
          } else {
            initialValues[c.key] = c.value;
          }
        });
      form.setFieldsValue(initialValues);
    } catch {
      message.error('Failed to load configuration');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { if (user?.role === 'SuperAdmin') fetchConfigs(); }, [user]);

  const onFinish = async (values: Record<string, unknown>) => {
    setSaving(true);
    try {
      // Merge changed GENERAL values back into the full config list
      const generalKeys = new Set(
        allConfigs.filter(c => c.category === 'GENERAL').map(c => c.key)
      );
      const updated = allConfigs.map(c => {
        if (!generalKeys.has(c.key)) return c;
        const raw = values[c.key];
        return { ...c, value: String(raw) };
      });
      await authApi.put(endpoints.systemConfig, updated);
      message.success('General settings saved');
      fetchConfigs();
    } catch {
      message.error('Failed to save settings');
    } finally {
      setSaving(false);
    }
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

  const generalConfigs = allConfigs.filter(c => c.category === 'GENERAL');

  // Group: toggles first, then the rest
  const boolConfigs = generalConfigs.filter(c => c.dataType === 'Boolean');
  const otherConfigs = generalConfigs.filter(c => c.dataType !== 'Boolean');

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">General Settings</h1>
          <p className="page-subtitle">Platform-wide flags and display preferences.</p>
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

      <Form form={form} layout="horizontal" onFinish={onFinish} disabled={loading || saving}>
        {/* Feature Flags */}
        {boolConfigs.length > 0 && (
          <div
            style={{
              background: 'var(--bg-surface)',
              border: '1px solid var(--border)',
              borderRadius: 8,
              overflow: 'hidden',
              marginBottom: 16,
            }}
          >
            <div style={{ padding: '12px 20px', borderBottom: '1px solid var(--border)' }}>
              <Text style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>
                Feature Flags
              </Text>
              <Text type="secondary" style={{ fontSize: 12, display: 'block' }}>
                Toggle on/off for immediate effect. No restart required.
              </Text>
            </div>
            {boolConfigs.map(c => <SettingRow key={c.key} config={c} />)}
          </div>
        )}

        {/* Other settings */}
        {otherConfigs.length > 0 && (
          <div
            style={{
              background: 'var(--bg-surface)',
              border: '1px solid var(--border)',
              borderRadius: 8,
              overflow: 'hidden',
            }}
          >
            <div style={{ padding: '12px 20px', borderBottom: '1px solid var(--border)' }}>
              <Text style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>
                App Settings
              </Text>
              <Text type="secondary" style={{ fontSize: 12, display: 'block' }}>
                Numeric limits and display values used across the platform.
              </Text>
            </div>
            {otherConfigs.map(c => <SettingRow key={c.key} config={c} />)}
          </div>
        )}
      </Form>
    </div>
  );
}
