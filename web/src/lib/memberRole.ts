import type { TeamMemberRole } from '@/types/team'

/** A colour per membership role, used for the role chip wherever it's shown. */
export const MEMBER_ROLE_COLOR: Record<TeamMemberRole, string> = {
  President: '#C9A227', // gold — leadership
  Coach: '#1565C0', // blue
  TechnicalStaff: '#00838F', // dark cyan
  Delegate: '#6A1B9A', // purple
  Player: '#2E7D32', // green
}
