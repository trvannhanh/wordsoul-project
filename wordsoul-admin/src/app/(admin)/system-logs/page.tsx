'use client';

import React, { useState, useEffect } from 'react';
import { Table, Tag, Drawer, Spin, Input, Select, Space, Button } from 'antd';
import { SyncOutlined, EyeOutlined } from '@ant-design/icons';
import { authApi, endpoints } from '@/services/api';

const { Search } = Input;
const { Option } = Select;

interface SystemLog {
  id: number;
  timestamp: string;
  method: string;
  path: string;
  statusCode: number;
  durationMs: number;
  ipAddress: string | null;
  userId: string | null;
}

interface SystemLogDetail extends SystemLog {
  requestPayload: string | null;
  responsePayload: string | null;
}

export default function SystemLogsPage() {
  const [logs, setLogs] = useState<SystemLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  
  // Filters
  const [methodFilter, setMethodFilter] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<number | null>(null);

  // Drawer state
  const [drawerVisible, setDrawerVisible] = useState(false);
  const [selectedLog, setSelectedLog] = useState<SystemLogDetail | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);

  const fetchLogs = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        pageIndex: page.toString(),
        pageSize: pageSize.toString(),
      });
      if (methodFilter) params.append('method', methodFilter);
      if (statusFilter) params.append('statusCode', statusFilter.toString());

      const res = await authApi.get(`${endpoints.systemLogs}?${params.toString()}`);
      setLogs(res.data.items);
      setTotal(res.data.totalItems);
    } catch (error) {
      console.error('Failed to fetch system logs:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLogs();
  }, [page, pageSize, methodFilter, statusFilter]);

  const viewLogDetail = async (id: number) => {
    setDrawerVisible(true);
    setDetailLoading(true);
    setSelectedLog(null);
    try {
      const res = await authApi.get(`${endpoints.systemLogs}/${id}`);
      setSelectedLog(res.data);
    } catch (error) {
      console.error('Failed to fetch log details:', error);
    } finally {
      setDetailLoading(false);
    }
  };

  const getMethodColor = (method: string) => {
    switch (method.toUpperCase()) {
      case 'GET': return 'blue';
      case 'POST': return 'green';
      case 'PUT': return 'orange';
      case 'DELETE': return 'red';
      default: return 'default';
    }
  };

  const getStatusColor = (status: number) => {
    if (status >= 200 && status < 300) return 'success';
    if (status >= 300 && status < 400) return 'processing';
    if (status >= 400 && status < 500) return 'warning';
    if (status >= 500) return 'error';
    return 'default';
  };

  const columns = [
    {
      title: 'Time',
      dataIndex: 'timestamp',
      key: 'timestamp',
      width: 180,
      render: (val: string) => new Date(val).toLocaleString(),
    },
    {
      title: 'Method',
      dataIndex: 'method',
      key: 'method',
      width: 90,
      render: (method: string) => (
        <Tag color={getMethodColor(method)}>{method}</Tag>
      ),
    },
    {
      title: 'Path',
      dataIndex: 'path',
      key: 'path',
      ellipsis: true,
      render: (text: string) => (
        <span style={{ fontFamily: 'monospace', fontSize: 13 }}>{text}</span>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'statusCode',
      key: 'statusCode',
      width: 90,
      render: (status: number) => (
        <Tag color={getStatusColor(status)}>{status}</Tag>
      ),
    },
    {
      title: 'Duration',
      dataIndex: 'durationMs',
      key: 'durationMs',
      width: 100,
      render: (ms: number) => `${ms} ms`,
    },
    {
      title: 'IP Address',
      dataIndex: 'ipAddress',
      key: 'ipAddress',
      width: 130,
    },
    {
      title: 'Action',
      key: 'action',
      width: 80,
      align: 'center' as const,
      render: (_: unknown, record: SystemLog) => (
        <Button 
          type="text" 
          icon={<EyeOutlined />} 
          onClick={() => viewLogDetail(record.id)}
          title="View Details"
        />
      ),
    },
  ];

  // A simple formatter to make JSON pretty if possible
  const formatPayload = (payload: string | null) => {
    if (!payload) return 'No payload';
    try {
      const obj = JSON.parse(payload);
      return JSON.stringify(obj, null, 2);
    } catch {
      return payload; // return as is if not valid JSON
    }
  };

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div>
          <h1 className="page-title">System Logs</h1>
          <p className="page-subtitle">Track HTTP requests, payloads, and API performance.</p>
        </div>
        <Space>
          <Select 
            placeholder="Method" 
            allowClear 
            style={{ width: 120 }}
            onChange={(val) => { setMethodFilter(val); setPage(1); }}
          >
            <Option value="GET">GET</Option>
            <Option value="POST">POST</Option>
            <Option value="PUT">PUT</Option>
            <Option value="DELETE">DELETE</Option>
          </Select>
          <Select 
            placeholder="Status Code" 
            allowClear 
            style={{ width: 120 }}
            onChange={(val) => { setStatusFilter(val ? parseInt(val) : null); setPage(1); }}
          >
            <Option value="200">200 OK</Option>
            <Option value="400">400 Bad Req</Option>
            <Option value="401">401 Unauth</Option>
            <Option value="403">403 Forbidden</Option>
            <Option value="404">404 Not Found</Option>
            <Option value="500">500 Server Err</Option>
          </Select>
          <Button icon={<SyncOutlined />} onClick={fetchLogs}>Refresh</Button>
        </Space>
      </div>

      <div style={{ background: 'var(--bg-surface)', padding: 16, borderRadius: 8, border: '1px solid var(--border)' }}>
        <Table
          columns={columns}
          dataSource={logs}
          rowKey="id"
          loading={loading}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            onChange: (p, s) => {
              setPage(p);
              setPageSize(s);
            },
          }}
          size="middle"
        />
      </div>

      {/* Detail Drawer */}
      <Drawer
        title="Request Details"
        placement="right"
        width={700}
        onClose={() => setDrawerVisible(false)}
        open={drawerVisible}
        styles={{ body: { padding: '20px' } }}
      >
        {detailLoading ? (
          <div style={{ textAlign: 'center', marginTop: 100 }}><Spin size="large" /></div>
        ) : selectedLog ? (
          <div>
            {/* Header info */}
            <div style={{ display: 'flex', gap: 10, marginBottom: 20, alignItems: 'center' }}>
              <Tag color={getMethodColor(selectedLog.method)} style={{ fontSize: 14, padding: '4px 10px' }}>
                {selectedLog.method}
              </Tag>
              <span style={{ fontSize: 16, fontWeight: 600, fontFamily: 'monospace' }}>
                {selectedLog.path}
              </span>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 24 }}>
              <div style={{ background: 'var(--bg-muted)', padding: 12, borderRadius: 6 }}>
                <div style={{ fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>Status Code</div>
                <div style={{ fontWeight: 600 }}><Tag color={getStatusColor(selectedLog.statusCode)}>{selectedLog.statusCode}</Tag></div>
              </div>
              <div style={{ background: 'var(--bg-muted)', padding: 12, borderRadius: 6 }}>
                <div style={{ fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>Duration</div>
                <div style={{ fontWeight: 600 }}>{selectedLog.durationMs} ms</div>
              </div>
              <div style={{ background: 'var(--bg-muted)', padding: 12, borderRadius: 6 }}>
                <div style={{ fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>IP Address</div>
                <div style={{ fontWeight: 600 }}>{selectedLog.ipAddress || 'Unknown'}</div>
              </div>
              <div style={{ background: 'var(--bg-muted)', padding: 12, borderRadius: 6 }}>
                <div style={{ fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>User ID</div>
                <div style={{ fontWeight: 600 }}>{selectedLog.userId || 'Anonymous'}</div>
              </div>
            </div>

            <h3 style={{ fontSize: 14, fontWeight: 600, marginBottom: 10, color: 'var(--text-primary)' }}>Request Payload</h3>
            <div style={{ 
              background: '#1e1e1e', 
              color: '#d4d4d4', 
              padding: 16, 
              borderRadius: 6, 
              fontFamily: 'monospace',
              fontSize: 13,
              whiteSpace: 'pre-wrap',
              wordBreak: 'break-all',
              maxHeight: 300,
              overflow: 'auto',
              marginBottom: 24
            }}>
              {formatPayload(selectedLog.requestPayload)}
            </div>

            <h3 style={{ fontSize: 14, fontWeight: 600, marginBottom: 10, color: 'var(--text-primary)' }}>Response Payload</h3>
            <div style={{ 
              background: '#1e1e1e', 
              color: '#d4d4d4', 
              padding: 16, 
              borderRadius: 6, 
              fontFamily: 'monospace',
              fontSize: 13,
              whiteSpace: 'pre-wrap',
              wordBreak: 'break-all',
              maxHeight: 400,
              overflow: 'auto'
            }}>
              {formatPayload(selectedLog.responsePayload)}
            </div>
          </div>
        ) : (
          <p>Log details not found.</p>
        )}
      </Drawer>
    </div>
  );
}
