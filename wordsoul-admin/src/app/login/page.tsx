'use client';

import React, { useState } from 'react';
import { Form, Input, Button, Card, Typography, message } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { api, endpoints } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';
import { useRouter } from 'next/navigation';

const { Title, Text } = Typography;

export default function LoginPage() {
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const router = useRouter();

  const onFinish = async (values: any) => {
    setLoading(true);
    try {
      const response = await api.post(endpoints.login, {
        usernameOrEmail: values.username,
        password: values.password,
      });

      const { accessToken } = response.data;
      
      try {
        login(accessToken);
        message.success('Login successful!');
      } catch (roleError: any) {
        message.error(roleError.message);
      }
      
    } catch (error: any) {
      if (error.response?.status === 401) {
        message.error('Invalid username or password');
      } else {
        message.error('An error occurred during login');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <Card className="w-full max-w-md shadow-lg border-0 rounded-xl overflow-hidden">
        <div className="text-center mb-8">
          <Title level={2} className="!mb-2 !mt-0 !text-blue-600">Vocamon Admin</Title>
          <Text type="secondary">System Administration Dashboard</Text>
        </div>

        <Form
          name="admin_login"
          initialValues={{ remember: true }}
          onFinish={onFinish}
          layout="vertical"
          size="large"
        >
          <Form.Item
            name="username"
            rules={[{ required: true, message: 'Please input your Username or Email!' }]}
          >
            <Input 
              prefix={<UserOutlined className="text-gray-400" />} 
              placeholder="Username or Email" 
            />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[{ required: true, message: 'Please input your Password!' }]}
          >
            <Input.Password
              prefix={<LockOutlined className="text-gray-400" />}
              placeholder="Password"
            />
          </Form.Item>

          <Form.Item className="mt-8 mb-0">
            <Button 
              type="primary" 
              htmlType="submit" 
              className="w-full" 
              loading={loading}
            >
              Log in
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
}
