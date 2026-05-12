export const CustomerStatus = {
  Active: 'Active',
  Blocked: 'Blocked',
} as const;

export type CustomerStatusType = (typeof CustomerStatus)[keyof typeof CustomerStatus];

export const SortOrder = {
  Asc: 'asc',
  Desc: 'desc',
} as const;

export type SortOrderType = (typeof SortOrder)[keyof typeof SortOrder];
