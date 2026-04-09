[anthropic-console]: https://console.anthropic.com/ "Anthropic Console"
[main-readme]: ../README.md "Main Project README"
[jupyter-docs]: https://jupyter.org/ "Jupyter Documentation"

# Python Implementation - Claude with Anthropic API

This is the Python implementation of the Claude API integration project.

## 🐍 Features

- **Jupyter notebook** with interactive examples
- **Direct API calls** using requests library
- **Environment variable** configuration
- **Simple and straightforward** approach for learning

## 🛠 Requirements

- Python 3.8+
- Jupyter Notebook or JupyterLab
- `requests` library
- `python-dotenv` library

## ⚙️ Setup

1. **Create and activate virtual environment**:
   ```bash
   python -m venv .venv
   .venv\Scripts\activate  # Windows
   # or
   source .venv/bin/activate  # macOS/Linux
   ```

2. **Install required packages**:
   ```bash
   pip install requests python-dotenv jupyter
   ```

3. **Set your API key** in the `.env` file:
   ```bash
   ANTHROPIC_API_KEY=your-api-key-here
   ```

## 🚀 Usage

1. **Start Jupyter**:
   ```bash
   jupyter notebook
   # or
   jupyter lab
   ```

2. **Open the notebook**:
   - Navigate to `001-requests.ipynb`
   - Run the cells to see the API integration examples

## 📋 Example Code

The notebook contains examples of:

### Basic API Call
```python
import requests
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

api_key = os.getenv('ANTHROPIC_API_KEY')
url = "https://api.anthropic.com/v1/messages"

headers = {
    "Content-Type": "application/json",
    "x-api-key": api_key,
    "anthropic-version": "2023-06-01"
}

payload = {
    "model": "claude-3-5-sonnet-20241022",
    "max_tokens": 200,
    "messages": [
        {
            "role": "user",
            "content": "What is quantum computing?"
        }
    ]
}

response = requests.post(url, json=payload, headers=headers)
result = response.json()
print(result["content"][0]["text"])
```

### Multiple Model Testing
The notebook demonstrates how to:
- Compare different Claude models
- Handle API responses
- Error handling and debugging
- Response time measurement

## 🔑 Configuration

Create a `.env` file in the python directory:
```
ANTHROPIC_API_KEY=your-api-key-here
```

**Get your API key from**: [Anthropic Console][anthropic-console]

## 📋 Example Output

```
API Response:
Quantum computing is a type of computation that harnesses quantum mechanical phenomena 
like superposition and entanglement to process information in ways that can solve 
certain complex problems much faster than classical computers.

Response time: 1.2 seconds
Model used: claude-3-5-sonnet-20241022
```

## 🛠 Development

### Interactive Development
- **Jupyter notebook** for step-by-step exploration
- **Live code cells** for immediate feedback
- **Markdown documentation** embedded with code
- **Easy experimentation** with different prompts and models

### Environment Management
- **Virtual environment** for isolation
- **Environment variables** for secure API key storage
- **Simple dependency management** with pip

## 📁 Project Structure

```
python/
├── .env                    # Environment variables (API key)
├── 001-requests.ipynb     # Main Jupyter notebook with examples
└── .venv/                 # Virtual environment (created during setup)
```

## 💡 Tips

- **Start with the notebook**: It's designed for learning and experimentation
- **Modify the prompts**: Try different questions and see how Claude responds
- **Compare models**: Experiment with different Claude models to see performance differences
- **Check rate limits**: Be mindful of API rate limits during development

---

For the complete project including .NET implementation, see the [Main Project README][main-readme].