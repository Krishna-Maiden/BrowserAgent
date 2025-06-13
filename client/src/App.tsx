import React, { useState } from 'react';

function App() {
  const [task, setTask] = useState('');
  const [status, setStatus] = useState('');

  const runTask = async () => {
    setStatus('Running...');
    const response = await fetch('http://localhost:5099/run-task', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ task }),
    });
    const result = await response.text();
    setStatus(result);
  };

  return (
    <div style={{ padding: 40 }}>
      <h2>AI Browser Agent</h2>
      <textarea rows={5} cols={60} value={task} onChange={(e) => setTask(e.target.value)} />
      <br />
      <button onClick={runTask}>Run Task</button>
      <p>{status}</p>
    </div>
  );
}

export default App;