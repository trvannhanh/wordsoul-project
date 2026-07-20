import React, { useState, useEffect } from 'react';
import { SettingsContext } from './SettingsContext';
import api from '../services/api';

export const SettingsProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [settings, setSettings] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchPublicSettings = async () => {
      try {
        const { data } = await api.get<Array<{ key: string; value: string }>>('/settings/public');
        const map: Record<string, string> = {};
        data.forEach((s) => {
          map[s.key] = s.value;
        });
        setSettings(map);

        // Apply Web App Title & Favicon dynamically
        const appName = map['WebAppName'] || 'VocaMon';
        const appSubtitle = map['WebAppSubtitle'] || 'Học từ vựng cùng thú cưng';
        document.title = `${appName} - ${appSubtitle}`;

        const favicon = map['WebAppFavicon'];
        if (favicon) {
          let link = document.querySelector("link[rel~='icon']") as HTMLLinkElement;
          if (!link) {
            link = document.createElement('link');
            link.rel = 'icon';
            document.head.appendChild(link);
          }
          link.href = favicon;
        }
      } catch (error) {
        console.error('Failed to load system settings:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchPublicSettings();
  }, []);

  const getSetting = (key: string, defaultValue = '') => {
    return settings[key] ?? defaultValue;
  };

  return (
    <SettingsContext.Provider value={{ settings, loading, getSetting }}>
      {children}
    </SettingsContext.Provider>
  );
};
