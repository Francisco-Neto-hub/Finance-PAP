import { Navigate } from 'react-router-dom';

const PrivateRoute = ({ children }) => {
  const token = localStorage.getItem('token');
  const userRole = localStorage.getItem('role'); // Guardaremos isto no login

  // Se não houver token ou não for Admin, volta para o login
  if (!token || userRole !== 'Admin') {
    return <Navigate to="/login" />;
  }

  return children;
};

export default PrivateRoute;