import React, { useState, useEffect } from 'react';
import './App.css'; 

interface VaultDto {
  id: number;
  name: string;
  description: string | null;
}

interface LoginResponse {
  accessToken: string;
}

function App() {
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [message, setMessage] = useState<string>('Please login.');
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [vaults, setVaults] = useState<VaultDto[]>([]);

  const API_BASE_URL = 'https://localhost:7107';

  const fetchVaults = async (currentToken: string) => {
    setMessage('Loading your vaults...');
    try {
      const response = await fetch(`${API_BASE_URL}/api/vaults`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${currentToken}`,
        },
      });

      if (response.ok) {
        const data = await response.json() as VaultDto[];
        setVaults(data);
        setMessage('');
      } else {
        setMessage('Session expired. Please login again.');
        handleLogout();
      }
    } catch (error) {
      console.error('Fetch vaults error:', error);
      setMessage('An error occurred while loading vaults.');
    }
  };

  const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault(); 
    setMessage('Logging in, please wait...');
    
    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });

      if (response.ok) {
        const data = await response.json() as LoginResponse;
        localStorage.setItem('token', data.accessToken);
        setToken(data.accessToken); 
        await fetchVaults(data.accessToken); 
        setMessage('');
      } else {
        setMessage('Login failed! Invalid email or password.');
      }
    } catch (error) {
      console.error('Login error:', error);
      setMessage('Could not connect to the API.');
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setVaults([]);
    setMessage('Logged out successfully.');
  };

  useEffect(() => {
    if (token) {
      fetchVaults(token);
    }
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <h1>PasswordApp</h1>
        
        {!token ? (
          <form onSubmit={handleLogin}>
            <h2>Login</h2>
            <div>
              <label>Email:</label>
              <input 
                type="email" 
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required 
              />
            </div>
            <div>
              <label>Password:</label>
              <input 
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            <button type="submit">Login</button>
            {message && <p>{message}</p>}
          </form>
        ) : (
          <div>
            <h2>Your Vaults</h2>
            <button onClick={handleLogout}>Logout</button>
            {message && <p>{message}</p>}
            
            {vaults.length > 0 ? (
              <ul>
                {vaults.map(vault => (
                  <li key={vault.id}>
                    <strong>{vault.name}</strong>
                    <p>{vault.description}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p>No vaults found.</p>
            )}
          </div>
        )}
      </header>
    </div>
  );
}

export default App;