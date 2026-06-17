"use client";

import { useState, type MouseEvent } from "react";
import {
  Button,
  Menu,
  MenuItem,
  ListItemText,
  ListItemIcon,
} from "@mui/material";
import TranslateIcon from "@mui/icons-material/Translate";
import CheckIcon from "@mui/icons-material/Check";
import { useTranslation } from "@/i18n/I18nProvider";
import { LANGUAGES, type Language } from "@/i18n/resources";

/** Header control to switch the UI language (UA / EN). */
const LanguageSwitcher = () => {
  const { language, setLanguage, t } = useTranslation();
  const [anchor, setAnchor] = useState<null | HTMLElement>(null);

  const current = LANGUAGES.find((l) => l.code === language) ?? LANGUAGES[0];

  const open = (e: MouseEvent<HTMLElement>) => setAnchor(e.currentTarget);
  const close = () => setAnchor(null);
  const pick = (code: Language) => {
    setLanguage(code);
    close();
  };

  return (
    <>
      <Button
        onClick={open}
        startIcon={<TranslateIcon />}
        aria-label={t("header.language")}
        sx={{ color: "#fff", mr: 1 }}
      >
        {current.short}
      </Button>
      <Menu anchorEl={anchor} open={!!anchor} onClose={close}>
        {LANGUAGES.map((l) => (
          <MenuItem
            key={l.code}
            selected={l.code === language}
            onClick={() => pick(l.code)}
          >
            <ListItemIcon sx={{ minWidth: 32, visibility: l.code === language ? "visible" : "hidden" }}>
              <CheckIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{l.label}</ListItemText>
          </MenuItem>
        ))}
      </Menu>
    </>
  );
};

export default LanguageSwitcher;
