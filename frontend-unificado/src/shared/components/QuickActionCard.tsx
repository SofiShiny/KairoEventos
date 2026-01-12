/**
 * QuickActionCard Component
 * Displays a quick action card for navigation
 */

import { Card, CardContent, CardActionArea, Typography, Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import EventIcon from '@mui/icons-material/Event';
import ConfirmationNumberIcon from '@mui/icons-material/ConfirmationNumber';
import PeopleIcon from '@mui/icons-material/People';
import AssessmentIcon from '@mui/icons-material/Assessment';

interface QuickActionCardProps {
  label: string;
  path: string;
  icon: string;
  description: string;
}

const iconMap = {
  event: EventIcon,
  ticket: ConfirmationNumberIcon,
  people: PeopleIcon,
  assessment: AssessmentIcon,
};

export function QuickActionCard({ label, path, icon, description }: QuickActionCardProps) {
  const navigate = useNavigate();
  const Icon = iconMap[icon as keyof typeof iconMap] || EventIcon;

  const handleClick = () => {
    navigate(path);
  };

  return (
    <Card
      sx={{
        height: '100%',
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 6,
        },
      }}
    >
      <CardActionArea onClick={handleClick} sx={{ height: '100%', p: 2 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <Icon sx={{ fontSize: 40, color: 'primary.main', mr: 2 }} />
            <Typography variant="h6" component="h3">
              {label}
            </Typography>
          </Box>
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}
