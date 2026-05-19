'use client';

import React, { useState, useEffect } from 'react';
import { Table, Spin, Tooltip } from 'antd';
import {
  UserOutlined,
  SafetyOutlined,
  CrownOutlined,
  CheckCircleFilled,
  CloseCircleFilled,
  MinusCircleFilled,
} from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

// ── Static permission definitions ────────────────────────────────────────────
type PermLevel = 'yes' | 'no' | 'self';

interface Permission {
  group: string;
  label: string;
  description: string;
  user: PermLevel;
  admin: PermLevel;
  superAdmin: PermLevel;
}

const PERMISSIONS: Permission[] = [
  // Profile
  { group: 'Profile', label: 'View own profile',       description: 'Read own user details, XP, AP, stats', user: 'yes', admin: 'yes', superAdmin: 'yes' },
  { group: 'Profile', label: 'Edit own profile',       description: 'Update username, avatar',               user: 'yes', admin: 'yes', superAdmin: 'yes' },
  { group: 'Profile', label: 'View any profile',       description: 'Look up other users by ID',             user: 'no',  admin: 'yes', superAdmin: 'yes' },
  // Users
  { group: 'Users',   label: 'List all users',         description: 'Paginated user list with filters',      user: 'no',  admin: 'yes', superAdmin: 'yes' },
  { group: 'Users',   label: 'Ban / Unban user',       description: 'Toggle isActive status',                user: 'no',  admin: 'yes', superAdmin: 'yes' },
  { group: 'Users',   label: 'Delete user',            description: 'Permanently remove account',            user: 'no',  admin: 'no',  superAdmin: 'yes' },
  { group: 'Users',   label: 'Assign Admin role',      description: 'Promote user to Admin',                 user: 'no',  admin: 'yes', superAdmin: 'yes' },
  { group: 'Users',   label: 'Assign SuperAdmin role', description: 'Promote user to SuperAdmin',            user: 'no',  admin: 'no',  superAdmin: 'yes' },
  { group: 'Users',   label: 'View activity logs',     description: 'Read user action history',              user: 'no',  admin: 'yes', superAdmin: 'yes' },
  // Vocabulary
  { group: 'Vocabulary', label: 'Create vocab set',    description: 'Publish a new vocabulary set',          user: 'yes', admin: 'yes', superAdmin: 'yes' },
  { group: 'Vocabulary', label: 'Edit any vocab set',  description: 'Modify sets created by others',         user: 'no',  admin: 'yes', superAdmin: 'yes' },
  { group: 'Vocabulary', label: 'Delete vocab set',    description: 'Remove a vocabulary set',               user: 'self', admin: 'yes', superAdmin: 'yes' },
  { group: 'Vocabulary', label: 'AI generate vocab',   description: 'Use AI to auto-create vocab entries',   user: 'yes', admin: 'yes', superAdmin: 'yes' },
  // Quests & Gyms
  { group: 'Game',    label: 'View quests',            description: 'Read quest & achievement definitions',  user: 'yes', admin: 'yes', superAdmin: 'yes' },
  { group: 'Game',    label: 'Manage quests',          description: 'Create/edit/delete quests',             user: 'no',  admin: 'yes', superAdmin: 'yes' },
  { group: 'Game',    label: 'View gym operations',    description: 'Read gym leader & challenge data',      user: 'yes', admin: 'yes', superAdmin: 'yes' },
  { group: 'Game',    label: 'Manage gyms',            description: 'Edit gym leaders and configurations',   user: 'no',  admin: 'yes', superAdmin: 'yes' },
  // System
  { group: 'System',  label: 'View system health',     description: 'Read system & uptime metrics',          user: 'no',  admin: 'no',  superAdmin: 'yes' },
  { group: 'System',  label: 'Edit system config',     description: 'Change app-wide configurations',        user: 'no',  admin: 'no',  superAdmin: 'yes' },
  { group: 'System',  label: 'Flush Redis cache',      description: 'Invalidate all cache entries',          user: 'no',  admin: 'no',  superAdmin: 'yes' },
  { group: 'System',  label: 'Database cleanup',       description: 'Archive stale records',                 user: 'no',  admin: 'no',  superAdmin: 'yes' },
];

// ── Role definitions ──────────────────────────────────────────────────────────
const ROLES = [
  {
    id: 'User',
    label: 'User',
    icon: <UserOutlined />,
    color: 'var(--text-secondary)',
    accentColor: '#64748b',
    bg: 'var(--bg-muted)',
    border: 'var(--border)',
    description: 'Regular platform member. Can learn vocabulary, complete quests, battle in PvP, and manage their own content.',
  },
  {
    id: 'Admin',
    label: 'Admin',
    icon: <SafetyOutlined />,
    color: '#0369a1',
    accentColor: '#0369a1',
    bg: '#f0f9ff',
    border: '#bae6fd',
    description: 'Platform moderator. Can manage users (ban/unban, assign roles), edit vocabulary sets, and configure game content.',
  },
  {
    id: 'SuperAdmin',
    label: 'SuperAdmin',
    icon: <CrownOutlined />,
    color: 'var(--accent)',
    accentColor: 'var(--accent)',
    bg: 'var(--accent-subtle)',
    border: 'var(--accent-border)',
    description: 'Full system access. All Admin permissions plus: delete users, manage system configuration, infrastructure tools.',
  },
];

// ── Permission cell ───────────────────────────────────────────────────────────
function PermCell({ level }: { level: PermLevel }) {
  if (level === 'yes') {
    return (
      <CheckCircleFilled style={{ fontSize: 15, color: 'var(--success)' }} />
    );
  }
  if (level === 'self') {
    return (
      <Tooltip title="Own resources only">
        <MinusCircleFilled style={{ fontSize: 15, color: '#f59e0b' }} />
      </Tooltip>
    );
  }
  return <CloseCircleFilled style={{ fontSize: 15, color: 'var(--border)' }} />;
}

// ── Stat card ─────────────────────────────────────────────────────────────────
function RoleCard({
  role,
  count,
  loading,
}: {
  role: (typeof ROLES)[number];
  count: number | null;
  loading: boolean;
}) {
  return (
    <div
      style={{
        flex: 1,
        minWidth: 200,
        background: 'var(--bg-surface)',
        border: `1px solid ${role.border}`,
        borderRadius: 10,
        padding: '16px 20px',
        display: 'flex',
        flexDirection: 'column',
        gap: 10,
      }}
    >
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <div
            style={{
              width: 30,
              height: 30,
              borderRadius: 7,
              background: role.bg,
              border: `1px solid ${role.border}`,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: role.color,
              fontSize: 14,
            }}
          >
            {role.icon}
          </div>
          <span style={{ fontSize: 14, fontWeight: 700, color: 'var(--text-primary)' }}>{role.label}</span>
        </div>

        {loading ? (
          <Spin size="small" />
        ) : (
          <div style={{ textAlign: 'right' }}>
            <div style={{ fontSize: 22, fontWeight: 800, color: role.color, lineHeight: 1 }}>
              {count ?? '—'}
            </div>
            <div style={{ fontSize: 11, color: 'var(--text-muted)' }}>members</div>
          </div>
        )}
      </div>

      {/* Description */}
      <p style={{ fontSize: 12, color: 'var(--text-muted)', margin: 0, lineHeight: 1.5 }}>
        {role.description}
      </p>
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────
export default function RolesPage() {
  const [counts, setCounts] = useState<Record<string, number | null>>({ User: null, Admin: null, SuperAdmin: null });
  const [countsLoading, setCountsLoading] = useState(true);

  useEffect(() => {
    const fetchCounts = async () => {
      setCountsLoading(true);
      try {
        const [usersRes, adminsRes, superAdminsRes] = await Promise.all([
          authApi.get(`${endpoints.users}?pageSize=500&role=User`),
          authApi.get(`${endpoints.users}?pageSize=500&role=Admin`),
          authApi.get(`${endpoints.users}?pageSize=500&role=SuperAdmin`),
        ]);
        setCounts({
          User: usersRes.data.length,
          Admin: adminsRes.data.length,
          SuperAdmin: superAdminsRes.data.length,
        });
      } catch {
        setCounts({ User: null, Admin: null, SuperAdmin: null });
      } finally {
        setCountsLoading(false);
      }
    };
    fetchCounts();
  }, []);

  // Group permissions by group
  const groups = Array.from(new Set(PERMISSIONS.map(p => p.group)));

  const columns = [
    {
      title: 'Permission',
      key: 'permission',
      width: 240,
      render: (_: unknown, record: Permission) => (
        <div>
          <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-primary)' }}>{record.label}</div>
          <div style={{ fontSize: 11, color: 'var(--text-muted)', marginTop: 1 }}>{record.description}</div>
        </div>
      ),
    },
    {
      title: () => (
        <div style={{ display: 'flex', alignItems: 'center', gap: 5, color: '#64748b' }}>
          <UserOutlined style={{ fontSize: 13 }} />
          <span>User</span>
        </div>
      ),
      key: 'user',
      width: 90,
      align: 'center' as const,
      render: (_: unknown, record: Permission) => <PermCell level={record.user} />,
    },
    {
      title: () => (
        <div style={{ display: 'flex', alignItems: 'center', gap: 5, color: '#0369a1' }}>
          <SafetyOutlined style={{ fontSize: 13 }} />
          <span>Admin</span>
        </div>
      ),
      key: 'admin',
      width: 90,
      align: 'center' as const,
      render: (_: unknown, record: Permission) => <PermCell level={record.admin} />,
    },
    {
      title: () => (
        <div style={{ display: 'flex', alignItems: 'center', gap: 5, color: 'var(--accent)' }}>
          <CrownOutlined style={{ fontSize: 13 }} />
          <span>SuperAdmin</span>
        </div>
      ),
      key: 'superAdmin',
      width: 110,
      align: 'center' as const,
      render: (_: unknown, record: Permission) => <PermCell level={record.superAdmin} />,
    },
  ];

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Role Management</h1>
          <p className="page-subtitle">Overview of platform roles, member counts, and permission matrix.</p>
        </div>
      </div>

      {/* Role Cards */}
      <div style={{ display: 'flex', gap: 14, flexWrap: 'wrap', marginBottom: 24 }}>
        {ROLES.map(role => (
          <RoleCard
            key={role.id}
            role={role}
            count={counts[role.id]}
            loading={countsLoading}
          />
        ))}
      </div>

      {/* Permission Matrix */}
      <div
        style={{
          background: 'var(--bg-surface)',
          border: '1px solid var(--border)',
          borderRadius: 8,
          overflow: 'hidden',
        }}
      >
        <div
          style={{
            padding: '12px 16px',
            borderBottom: '1px solid var(--border)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}
        >
          <div>
            <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-primary)' }}>
              Permission Matrix
            </span>
            <span style={{ fontSize: 12, color: 'var(--text-muted)', marginLeft: 8 }}>
              {PERMISSIONS.length} permissions across {groups.length} groups
            </span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 14, fontSize: 11, color: 'var(--text-muted)' }}>
            <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <CheckCircleFilled style={{ color: 'var(--success)', fontSize: 12 }} /> Allowed
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <MinusCircleFilled style={{ color: '#f59e0b', fontSize: 12 }} /> Own only
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <CloseCircleFilled style={{ color: 'var(--border)', fontSize: 12 }} /> Denied
            </span>
          </div>
        </div>

        {groups.map(group => {
          const groupPerms = PERMISSIONS.filter(p => p.group === group);
          return (
            <div key={group}>
              {/* Group label row */}
              <div
                style={{
                  padding: '6px 16px',
                  background: 'var(--bg-muted)',
                  borderBottom: '1px solid var(--border)',
                  fontSize: 10,
                  fontWeight: 700,
                  textTransform: 'uppercase',
                  letterSpacing: '0.07em',
                  color: 'var(--text-muted)',
                }}
              >
                {group}
              </div>
              <Table
                columns={columns}
                dataSource={groupPerms}
                rowKey="label"
                size="small"
                pagination={false}
                showHeader={group === groups[0]}
                style={{ fontSize: 12 }}
                className="permission-table"
              />
            </div>
          );
        })}
      </div>
    </div>
  );
}
