/**
 * Role definitions và helpers
 */
export const ROLES = {
  ADMIN: 'Admin',
  MEMBER: 'Member',
  REFEREE: 'Referee',
  TREASURER: 'Treasurer'
}

export const ROLE_LABELS = {
  [ROLES.ADMIN]: 'Quản trị viên',
  [ROLES.MEMBER]: 'Thành viên',
  [ROLES.REFEREE]: 'Trọng tài',
  [ROLES.TREASURER]: 'Thủ quỹ'
}

export const getRoleLabel = (role) => {
  return ROLE_LABELS[role] || role
}

export const getRoleBadgeClass = (role) => {
  const classes = {
    [ROLES.ADMIN]: 'badge-danger',
    [ROLES.MEMBER]: 'badge-primary',
    [ROLES.REFEREE]: 'badge-warning',
    [ROLES.TREASURER]: 'badge-success'
  }
  return classes[role] || 'badge-info'
}
