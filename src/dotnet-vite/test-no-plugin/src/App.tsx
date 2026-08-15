import { useState } from 'react'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="app">
      <h1>test-no-plugin</h1>
      <p>
        This React SPA does <strong>not</strong> use the <code>vite-dotnet</code> npm package. It
        vendors the Vite.NET plugin locally (<code>./vite-dotnet</code>), following the
        &ldquo;Using the Plugin Without npm&rdquo; guide.
      </p>
      <button onClick={() => setCount((c) => c + 1)}>count is {count}</button>
      <p>Edit <code>src/App.tsx</code> and save to test HMR.</p>
    </div>
  )
}

export default App
