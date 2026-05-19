'use client';

import React, { useState, useEffect } from 'react';
import { Button, Alert, App } from 'antd';
import { ReloadOutlined } from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';
import dayjs from 'dayjs';

interface HealthStats {
  status: string;
  uptime: string;
  database: string;
  timestamp: string;
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
  const [health, setHealth] = useState<HealthStats | null>(null);
  const [loading, setLoading] = useState(false);
  const { message } = App.useApp();

  const fetchHealth = async () => {
    setLoading(true);
    try {
      const { data } = await authApi.get<HealthStats>(endpoints.systemHealth);
      setHealth(data);
    } catch {
      message.error('Failed to load system health');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { if (user?.role === 'SuperAdmin') fetchHealth(); }, [user]);

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

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">System Health</h1>
          <p className="page-subtitle">Monitor real-time system status, services, and run maintenance tasks.</p>
        </div>
        <Button size="small" icon={<ReloadOutlined />} onClick={fetchHealth} loading={loading}>
          Refresh
        </Button>
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
            status: (health?.status === 'Healthy' ? 'healthy' : health ? 'error' : 'neutral') as 'healthy' | 'error' | 'neutral',
          },
          {
            label: 'Database',
            value: health?.database ?? '—',
            status: (health?.database === 'Healthy' ? 'healthy' : health ? 'error' : 'neutral') as 'healthy' | 'error' | 'neutral',
          },
          {
            label: 'Uptime',
            value: '99.9%',
            status: 'healthy' as const,
          },
          {
            label: 'Last Checked',
            value: health ? dayjs(health.timestamp).format('HH:mm:ss') : '—',
            status: 'neutral' as const,
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

      {/* Maintenance & Monitor */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 14 }}>
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

        {/* WebSocket Monitor */}
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
              <HealthRow key={item.label} label={item.label} value={item.value} status={item.status as 'healthy'} />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

