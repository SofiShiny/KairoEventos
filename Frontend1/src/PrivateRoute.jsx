
import React from "react";
import { useKeycloak } from "@react-keycloak/web";

const PrivateRoute = ({ children }) => {
  const { keycloak, initialized } = useKeycloak();

  if (!initialized) {
    return <div style={{ padding: 24 }}>Cargando autenticación...</div>;
  }

  if (!keycloak.authenticated) {
    return (
      <div style={{ padding: 24 }}>
        <h2>Acceso restringido</h2>
        <p>Necesitas iniciar sesión para ver esta página.</p>
        <button onClick={() => keycloak.login()}>
          Iniciar Sesión
        </button>
      </div>
    );
  }

  return children;
};

export default PrivateRoute;
