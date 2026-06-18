"use client";

import { useState, type MouseEvent } from "react";
import {
  Button,
  Menu,
  MenuItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Typography,
  Box,
} from "@mui/material";
import BadgeIcon from "@mui/icons-material/Badge";
import CheckIcon from "@mui/icons-material/Check";
import { useRole } from "@/access/RoleProvider";
import { ROLE_ORDER, ROLES, type RoleCode } from "@/access/roles";
import { useTranslation } from "@/i18n/I18nProvider";

/**
 * Header control to switch the active role. With no real identity provider yet
 * this simulates "signed in as" so every access level can be demonstrated; the
 * backend must still enforce the same rules in production.
 */
export default function RoleSwitcher() {
  const { t } = useTranslation();
  const { role, def, setRole } = useRole();
  const [anchor, setAnchor] = useState<null | HTMLElement>(null);

  const open = (e: MouseEvent<HTMLElement>) => setAnchor(e.currentTarget);
  const close = () => setAnchor(null);
  const pick = (code: RoleCode) => {
    setRole(code);
    close();
  };

  return (
    <>
      <Button
        onClick={open}
        startIcon={<BadgeIcon />}
        aria-label={t("access.switchRole")}
        sx={{ color: "#fff", textTransform: "none" }}
      >
        <Box sx={{ display: "flex", alignItems: "center", gap: 0.75 }}>
          <Chip
            label={def.badge}
            size="small"
            sx={{
              height: 20,
              bgcolor: "rgba(255,255,255,0.22)",
              color: "#fff",
              fontWeight: 700,
            }}
          />
          <Typography variant="body2" sx={{ fontWeight: 600 }} noWrap>
            {t(`roles.list.${role}.name`)}
          </Typography>
        </Box>
      </Button>

      <Menu
        anchorEl={anchor}
        open={!!anchor}
        onClose={close}
        slotProps={{ paper: { sx: { maxWidth: 380 } } }}
      >
        {ROLE_ORDER.map((code) => (
          <MenuItem
            key={code}
            selected={code === role}
            onClick={() => pick(code)}
            sx={{ alignItems: "flex-start", py: 1 }}
          >
            <ListItemIcon
              sx={{
                minWidth: 30,
                mt: 0.5,
                visibility: code === role ? "visible" : "hidden",
              }}
            >
              <CheckIcon fontSize="small" color="primary" />
            </ListItemIcon>
            <Chip
              label={ROLES[code].badge}
              size="small"
              variant="outlined"
              sx={{ mr: 1, mt: 0.25, minWidth: 44, fontWeight: 700 }}
            />
            <ListItemText
              primary={t(`roles.list.${code}.name`)}
              secondary={t(`roles.list.${code}.level`)}
              slotProps={{
                primary: { sx: { fontWeight: 600, whiteSpace: "normal" } },
                secondary: { variant: "caption" },
              }}
            />
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
