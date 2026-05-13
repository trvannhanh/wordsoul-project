'use client';

import React, { useState } from 'react';
import { Form, Input, Button, App, Divider } from 'antd';
import { UserOutlined, LockOutlined, ArrowRightOutlined } from '@ant-design/icons';
import { api, endpoints } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

export default function LoginPage() {
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const { message } = App.useApp();

  const onFinish = async (values: { username: string; password: string }) => {
    setLoading(true);
    try {
      const response = await api.post(endpoints.login, {
        username: values.username,
        password: values.password,
      });
      const { accessToken } = response.data;
      try {
        login(accessToken);
      } catch (roleError: any) {
        message.error(roleError.message);
      }
    } catch (error: any) {
      if (error.response?.status === 401) {
        message.error('Invalid username or password.');
      } else {
        message.error('Login failed. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'stretch',
        background: 'var(--bg-base)',
      }}
    >
      {/* ── Left panel (desktop info) ───────────────────────────────────────── */}
      <div
        style={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          padding: '48px 64px',
          background: 'var(--bg-surface)',
          borderRight: '1px solid var(--border)',
          maxWidth: 480,
        }}
        className="login-left-panel"
      >
        {/* Logo */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 40 }}>
          <svg width="28" height="28" viewBox="0 0 28 28" fill="none">
            <rect width="28" height="28" rx="8" fill="var(--accent)" opacity="0.12" />
            <path d="M7 9h14M7 14h8M7 19h11" stroke="var(--accent)" strokeWidth="2" strokeLinecap="round" />
          </svg>
          <span style={{ fontSize: 16, fontWeight: 700, color: 'var(--text-primary)' }}>
            Vocamon
          </span>
          <span
            style={{
              fontSize: 10,
              fontWeight: 600,
              color: 'var(--accent)',
              background: 'var(--accent-subtle)',
              border: '1px solid var(--accent-border)',
              borderRadius: 3,
              padding: '1px 5px',
            }}
          >
            Admin
          </span>
        </div>

        <h1
          style={{
            fontSize: 22,
            fontWeight: 700,
            color: 'var(--text-primary)',
            margin: '0 0 8px',
            lineHeight: 1.3,
          }}
        >
          Administration Portal
        </h1>
        <p style={{ fontSize: 14, color: 'var(--text-secondary)', margin: '0 0 32px', lineHeight: 1.6 }}>
          Manage users, vocabulary content, game configuration, and monitor system health.
        </p>

        <Divider style={{ borderColor: 'var(--border)', margin: '0 0 24px' }} />

        <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          {[
            { icon: '👥', text: 'User management & role assignment' },
            { icon: '📚', text: 'Vocabulary library & AI generation' },
            { icon: '⚙️', text: 'SRS algorithm & system configuration' },
          ].map(item => (
            <div key={item.text} style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span style={{ fontSize: 16, width: 22, textAlign: 'center' }}>{item.icon}</span>
              <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{item.text}</span>
            </div>
          ))}
        </div>

        <div
          style={{
            marginTop: 'auto',
            paddingTop: 32,
            fontSize: 12,
            color: 'var(--text-muted)',
            display: 'flex',
            alignItems: 'center',
            gap: 6,
          }}
        >
          <span
            className="status-dot healthy live"
            style={{ display: 'inline-block', width: 7, height: 7, borderRadius: '50%', background: 'var(--success)' }}
          />
          Restricted access — authorized administrators only
        </div>
      </div>

      {/* ── Right panel (login form) ────────────────────────────────────────── */}
      <div
        style={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          padding: '48px 40px',
        }}
      >
        <div style={{ width: '100%', maxWidth: 360 }}>
          <h2
            style={{
              fontSize: 18,
              fontWeight: 600,
              color: 'var(--text-primary)',
              margin: '0 0 4px',
            }}
          >
            Sign in to continue
          </h2>
          <p style={{ fontSize: 13, color: 'var(--text-muted)', margin: '0 0 28px' }}>
            Enter your administrator credentials below.
          </p>

          <Form
            name="admin_login"
            onFinish={onFinish}
            layout="vertical"
            size="large"
            requiredMark={false}
          >
            <Form.Item
              name="username"
              label="Username or Email"
              rules={[{ required: true, message: 'Required' }]}
              style={{ marginBottom: 16 }}
            >
              <Input
                prefix={<UserOutlined style={{ color: 'var(--text-muted)' }} />}
                placeholder="admin@vocamon.com"
              />
            </Form.Item>

            <Form.Item
              name="password"
              label="Password"
              rules={[{ required: true, message: 'Required' }]}
              style={{ marginBottom: 24 }}
            >
              <Input.Password
                prefix={<LockOutlined style={{ color: 'var(--text-muted)' }} />}
                placeholder="••••••••"
              />
            </Form.Item>

            <Form.Item style={{ marginBottom: 0 }}>
              <Button
                type="primary"
                htmlType="submit"
                loading={loading}
                icon={<ArrowRightOutlined />}
                iconPosition="end"
                style={{ width: '100%', height: 42, fontWeight: 600 }}
              >
                Sign in
              </Button>
            </Form.Item>
          </Form>

          <p
            style={{
              marginTop: 24,
              fontSize: 12,
              color: 'var(--text-muted)',
              textAlign: 'center',
              lineHeight: 1.5,
            }}
          >
            Having trouble? Contact your system administrator.
          </p>
        </div>
      </div>

      {/* Hide left panel on small screens */}
      <style>{`
        @media (max-width: 768px) {
          .login-left-panel { display: none !important; }
        }
      `}</style>
    </div>
  );
}
