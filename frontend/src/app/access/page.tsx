"use client";

import {
  Box,
  Container,
  Card,
  CardContent,
  Stack,
  Typography,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  TableContainer,
  Chip,
  Button,
  Alert,
} from "@mui/material";
import LoginIcon from "@mui/icons-material/Login";
import AppHeader from "@/components/AppHeader";
import { useRole } from "@/access/RoleProvider";
import { ROLE_ORDER, ROLES } from "@/access/roles";
import { useTranslation } from "@/i18n/I18nProvider";

export default function AccessPage() {
  const { t } = useTranslation();
  const { role, setRole } = useRole();

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppHeader showHome />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Stack spacing={3}>
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 800 }}>
              {t("access.title")}
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mt: 1 }}>
              {t("access.subtitle")}
            </Typography>
          </Box>

          <Alert severity="info">{t("access.simulatorNote")}</Alert>

          <Card variant="outlined">
            <CardContent>
              <TableContainer>
                <Table size="small" sx={{ minWidth: 880 }}>
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 700 }}>{t("access.colCode")}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{t("access.colRole")}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{t("access.colLevel")}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{t("access.colSees")}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{t("access.colRestriction")}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{t("access.colCategory")}</TableCell>
                      <TableCell />
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {ROLE_ORDER.map((code) => {
                      const active = code === role;
                      return (
                        <TableRow
                          key={code}
                          hover
                          selected={active}
                          sx={{ "& td": { verticalAlign: "top" } }}
                        >
                          <TableCell>
                            <Chip
                              label={ROLES[code].badge}
                              size="small"
                              color={active ? "primary" : "default"}
                              variant={active ? "filled" : "outlined"}
                              sx={{ fontWeight: 700 }}
                            />
                          </TableCell>
                          <TableCell sx={{ fontWeight: 600, minWidth: 160 }}>
                            {t(`roles.list.${code}.name`)}
                          </TableCell>
                          <TableCell sx={{ whiteSpace: "nowrap" }}>
                            {t(`roles.list.${code}.level`)}
                          </TableCell>
                          <TableCell sx={{ color: "text.secondary", minWidth: 240 }}>
                            {t(`roles.list.${code}.sees`)}
                          </TableCell>
                          <TableCell sx={{ color: "text.secondary", minWidth: 220 }}>
                            {t(`roles.list.${code}.restriction`)}
                          </TableCell>
                          <TableCell sx={{ whiteSpace: "nowrap" }}>
                            {t(`roles.categories.${ROLES[code].category}`)}
                          </TableCell>
                          <TableCell>
                            {active ? (
                              <Chip size="small" color="primary" label={t("access.youAreHere")} />
                            ) : (
                              <Button
                                size="small"
                                startIcon={<LoginIcon />}
                                onClick={() => setRole(code)}
                              >
                                {t("access.switchRole")}
                              </Button>
                            )}
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Stack>
      </Container>
    </Box>
  );
}
