'use client';

import React from 'react';
import { Typography, Row, Col, Card, Statistic } from 'antd';
import { useAuth } from '@/contexts/AuthContext';
import { UserOutlined, BookOutlined, RiseOutlined } from '@ant-design/icons';

const { Title, Paragraph } = Typography;

export default function DashboardPage() {
  const { user } = useAuth();

  return (
    <div>
      <div className="mb-6">
        <Title level={2} className="!mt-0">Welcome back, {user?.name}!</Title>
        <Paragraph type="secondary">
          Here is what's happening with your platform today.
        </Paragraph>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={8}>
          <Card bordered={false} className="shadow-sm">
            <Statistic
              title="Active Users"
              value={1128}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card bordered={false} className="shadow-sm">
            <Statistic
              title="Total Vocabularies"
              value={93}
              prefix={<BookOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card bordered={false} className="shadow-sm">
            <Statistic
              title="Avg Retention Rate (7-day)"
              value={42.5}
              precision={2}
              suffix="%"
              prefix={<RiseOutlined />}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>
      
      {/* Real dashboard charts will be implemented here later */}
      <div className="mt-8 flex items-center justify-center h-64 bg-gray-50 border border-dashed border-gray-200 rounded-lg">
        <p className="text-gray-400">Charts and Detailed Analytics will be loaded here.</p>
      </div>
    </div>
  );
}
