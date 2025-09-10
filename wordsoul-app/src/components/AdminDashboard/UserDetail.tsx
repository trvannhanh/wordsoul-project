import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import {
  getUserById,
  updateUser,
  assignRoleToUser,
  removeRoleFromUser,
  getUserActivities,
} from '../../services/userService';
import type { ActivityLogDto, UserDto } from '../../types/Dto';


const UserDetail = () => {
  const { userId } = useParams<{ userId: string }>();
  const [user, setUser] = useState<UserDto | null>(null);
  const [activities, setActivities] = useState<ActivityLogDto[]>([]);
  const [role, setRole] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const userData = await getUserById(Number(userId));
        const activitiesData = await getUserActivities(Number(userId));
        setUser(userData);
        setActivities(activitiesData);
      } catch (error) {
        console.error('Error fetching user details:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [userId]);

  const handleUpdate = async () => {
    if (user) {
      await updateUser(Number(userId), user);
      alert('User updated successfully');
    }
  };

  const handleAssignRole = async () => {
    if (role && user) {
      await assignRoleToUser(Number(userId), role);
      alert('Role assigned successfully');
      window.location.reload();
    }
  };

  const handleRemoveRole = async () => {
    if (user?.role && user.role !== 'User') {
      await removeRoleFromUser(Number(userId), user.role);
      alert('Role removed successfully');
      window.location.reload();
    }
  };

  if (loading) return <div className="text-center">Loading...</div>;

  return (
    <div className="bg-white p-6 rounded-lg shadow">
      <h2 className="text-2xl font-semibold mb-6">User Details - ID: {userId}</h2>
      {user && (
        <div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700">Username</label>
              <input
                type="text"
                value={user.username}
                onChange={(e) => setUser({ ...user, username: e.target.value })}
                className="mt-1 block w-full border-gray-300 rounded-md shadow-sm"
              />
            </div>
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700">Role</label>
              <p className="mt-1">{user.role}</p>
              <input
                type="text"
                value={role}
                onChange={(e) => setRole(e.target.value)}
                placeholder="Enter role (e.g., Admin)"
                className="mt-2 block w-full border-gray-300 rounded-md shadow-sm"
              />
              <div className="mt-2">
                <button
                  onClick={handleAssignRole}
                  className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700"
                >
                  Assign Role
                </button>
                {user.role !== 'User' && (
                  <button
                    onClick={handleRemoveRole}
                    className="ml-2 bg-red-600 text-white px-4 py-2 rounded-md hover:bg-red-700"
                  >
                    Remove Role
                  </button>
                )}
              </div>
            </div>
          </div>
          <button
            onClick={handleUpdate}
            className="mt-4 bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700"
          >
            Save Changes
          </button>
        </div>
      )}
      <h3 className="text-xl font-semibold mt-6 mb-4">Activities</h3>
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Action</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Details</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Timestamp</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {activities.map((activity) => (
              <tr key={activity.id}>
                <td className="px-6 py-4 whitespace-nowrap">{activity.action}</td>
                <td className="px-6 py-4 whitespace-nowrap">{activity.details}</td>
                <td className="px-6 py-4 whitespace-nowrap">{new Date(activity.timestamp).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default UserDetail;