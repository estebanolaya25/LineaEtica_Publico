export interface MenuItem {
  label: string;
  icon: string;
  route: string;
  roles: string[];
  queryParams?: Record<string, string>;
  groupLabel?: string;
}

export const MENU_ITEMS: MenuItem[] = [
  {
    label: 'Inicio',
    icon:  'bi-house-fill',
    route: '/pages/inicio',
    roles: [],
  },
  {
    label: 'Mis Denuncias',
    icon:  'bi-file-earmark-text-fill',
    route: '/pages/mis-denuncias',
    roles: ['Denunciante'],
  },
  {
    label:      'Dashboard',
    icon:       'bi-speedometer2',
    route:      '/pages/admin/dashboard',
    roles:      ['Consultor', 'Jefe del proceso'],
    groupLabel: 'Administración',
  },
  {
    label:      'Por Revisar',
    icon:       'bi-clock-history',
    route:      '/pages/admin/bandeja',
    queryParams: { seccion: 'pendientes' },
    roles:      ['Consultor', 'Jefe del proceso'],
    groupLabel: 'Bandeja',
  },
  {
    label:      'Historial',
    icon:       'bi-archive-fill',
    route:      '/pages/admin/bandeja',
    queryParams: { seccion: 'historial' },
    roles:      ['Consultor', 'Jefe del proceso'],
  },
  {
    label:      'Investigaciones',
    icon:       'bi-shield-check',
    route:      '/pages/admin/investigaciones',
    roles:      ['Consultor', 'Jefe del proceso'],
  },
  {
    label:      'Dashboard',
    icon:       'bi-speedometer2',
    route:      '/pages/investigador/dashboard',
    roles:      ['Investigador'],
    groupLabel: 'Investigación',
  },
  {
    label:      'Mis Investigaciones',
    icon:       'bi-search',
    route:      '/pages/investigador/bandeja',
    roles:      ['Investigador'],
  },
  {
    label:      'Dashboard',
    icon:       'bi-speedometer2',
    route:      '/pages/registrador/dashboard',
    roles:      ['Registrador'],
    groupLabel: 'Registro de Denuncias',
  },
  {
    label:      'Montar Denuncia',
    icon:       'bi-envelope-plus-fill',
    route:      '/pages/registrador/montar-denuncia',
    roles:      ['Registrador'],
  },
  {
    label:      'Gestión de Usuarios',
    icon:       'bi-people-fill',
    route:      '/pages/admin/usuarios',
    roles:      ['Consultor', 'Jefe del proceso'],
    groupLabel: 'Configuración',
  },
  {
    label:      'Manual de Usuario',
    icon:       'bi-journal-bookmark-fill',
    route:      '/pages/admin/manual',
    roles:      ['Jefe del proceso'],
  },
];
