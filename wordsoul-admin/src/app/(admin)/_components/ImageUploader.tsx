'use client';

import React, { useState } from 'react';
import { Upload, Typography } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import type { UploadProps } from 'antd';

const { Text } = Typography;

interface ImageUploaderProps {
  /** Current image URL — shown as preview in edit mode when no new file is selected */
  value?: string;
  onChange?: (file: File | null) => void;
  size?: number; // px, default 102
}

const ACCEPT = 'image/jpeg,image/png,image/webp';
const MAX_MB = 2;

export default function ImageUploader({ value, onChange, size = 102 }: ImageUploaderProps) {
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleChange: UploadProps['onChange'] = (info) => {
    const raw = info.file.originFileObj ?? (info.file as unknown as File);
    if (!raw) return;

    setError(null);

    if (!raw.type.startsWith('image/')) {
      setError('Only JPEG / PNG / WebP allowed');
      return;
    }
    if (raw.size / 1024 / 1024 > MAX_MB) {
      setError(`Max ${MAX_MB} MB`);
      return;
    }

    // Revoke previous object URL to avoid leaks
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    const url = URL.createObjectURL(raw);
    setPreviewUrl(url);
    onChange?.(raw);
  };

  const handleRemove = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    setPreviewUrl(null);
    setError(null);
    onChange?.(null);
  };

  const displayUrl = previewUrl ?? value ?? null;

  return (
    <div>
      <Upload
        listType="picture-card"
        showUploadList={false}
        beforeUpload={() => false}
        onChange={handleChange}
        accept={ACCEPT}
        style={{ width: size, height: size }}
      >
        {displayUrl ? (
          <div style={{ position: 'relative', width: '100%', height: '100%' }}>
            <img
              src={displayUrl}
              alt="cover"
              style={{ width: '100%', height: '100%', objectFit: 'cover', borderRadius: 6 }}
            />
            <div
              onClick={handleRemove}
              style={{
                position: 'absolute',
                inset: 0,
                background: 'rgba(0,0,0,0.45)',
                borderRadius: 6,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                opacity: 0,
                transition: 'opacity 0.2s',
              }}
              className="img-remove-overlay"
            >
              <DeleteOutlined style={{ color: '#fff', fontSize: 18 }} />
            </div>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 4 }}>
            <PlusOutlined style={{ fontSize: 16, color: 'var(--text-muted)' }} />
            <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>Upload</span>
          </div>
        )}
      </Upload>
      {error && (
        <Text type="danger" style={{ fontSize: 11, display: 'block', marginTop: 4 }}>
          {error}
        </Text>
      )}
      <Text type="secondary" style={{ fontSize: 10, display: 'block', marginTop: 2 }}>
        JPEG · PNG · WebP · max {MAX_MB} MB
      </Text>
      <style>{`
        .ant-upload-select:hover .img-remove-overlay { opacity: 1 !important; }
      `}</style>
    </div>
  );
}
