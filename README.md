
# AI Selector Engine for UI Automation

## Overview
This project implements an AI-powered element resolution engine for robust UI automation using Playwright. It supports dynamic selector suggestion using local or OpenAI-based LLMs and enables resilient testing across changing UI structures.

## Features
- ✅ Dynamic Selector Resolution using AI
- ✅ Support for both OpenAI and Ollama (LLaMA3) LLMs
- ✅ Fallback logic with in-memory selector memory
- ✅ Clean integration with PlaywrightExecutor
- ✅ AppSettings toggle between local/remote LLM
- ✅ Login action inference with AI-based element matching

## Folder Structure
```
AgentCore/
├── Automation/
│   └── PlaywrightExecutor.cs
├── ElementResolution/
│   ├── ISelectorMemory.cs
│   ├── InMemorySelectorMemory.cs
│   ├── ILlmSelectorSuggester.cs
│   ├── OpenAiSelectorSuggester.cs
│   ├── OllamaSelectorSuggester.cs
│   └── SelectorResolver.cs
├── Prompts/
│   └── llm_prompt_template.txt
└── Program.cs
```

## Configuration
In `appsettings.json`, configure the selector resolution engine:
```json
"LLM": {
  "UseLocal": true,
  "Ollama": {
    "Model": "llama3",
    "Url": "http://localhost:11434/api/generate"
  },
  "OpenAI": {
    "ApiKey": "your-api-key",
    "Model": "gpt-4"
  }
}
```

## Dependencies
- Microsoft.Playwright
- OpenAI .NET SDK
- LangChain.NET (optional)
- Ollama running locally for local inference

## Future Enhancements
- Add multi-model fallback (OpenRouter, Mistral)
- Incorporate confidence scores for selector suggestions
- Auto-debugging on failed selectors
- Long-term persistent selector memory (Redis/DB)
