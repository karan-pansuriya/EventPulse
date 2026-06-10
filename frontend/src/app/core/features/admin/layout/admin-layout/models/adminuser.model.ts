export interface OrganizerResponse {
    id: number;
    name: string;
    email: string;
  }
  
  export interface UserListResponse {
    id: number;
    name: string;
    email: string;
    phone: string | null;
    isActive: boolean;
    roles: string[];
    roleIds: number[];
    createdAt: string;
  }

  export interface RoleResponse {
    id: number;
    name: string;
  }