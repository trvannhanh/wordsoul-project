import type { UserDashboardDto } from "../types/Dto";
import { authApi, endpoints } from "./api";

export const getUserDashboard = async (): Promise<UserDashboardDto> => {
  const res = await authApi.get<UserDashboardDto>(endpoints.userDashboard);
  return res.data;
};