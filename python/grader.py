"""Grader: evaluates Claude responses for prompt evaluation."""

from __future__ import annotations

import json
import re

from anthropic_client import AnthropicClient
from anthropic_message import AssistantMessage, UserMessage


class Grader:
    """Evaluates a Claude response for a given task ID.

    Returns a score as a float in the range [1.0, 10.0],
    or 0.0 if the task ID is not found for the current version.
    The version is extracted from the prompt template comment (# vX.Y).
    dataset is stored for potential use in grading logic.
    """

    def __init__(self, client: AnthropicClient):
        self._client = client

    def mocked_score(self, testCase: dict[str, str], response: str) -> float:
        task_id = testCase.get("task_id", "")
        return self._scores_by_version.get(self._version, {}).get(task_id, 0.0)

    def scoreByModel(self, testCase: dict[str, str], response: str) -> dict:
        eval_prompt = f"""
        You are an expert code reviewer. Evaluate this AI-generated solution.

        Original Task:
        <task>
        {testCase["task"]}
        </task>

        Solution to evaluate:
        <solution>
        {response}
        </solution>

        Criteria you should use to evaluate the solution:
        <criteria>
        {testCase.get("solution_criteria", "overall correctness, efficiency, and code quality.")}
        </criteria>
        
        Provide your evaluation as a structured JSON object with:
        - "strengths": An array of 1-3 key strengths
        - "weaknesses": An array of 1-3 key areas for improvement
        - "reasoning": A concise explanation of your assessment
        - "score": A number between 1-10

        Respond with JSON only, no additional text. Keep your response concise and focused on the evaluation criteria.
        Example response format:
        ```json
        {{
            "strengths": ["Well-structured code", "Efficient algorithm"],
            "weaknesses": ["Lacks error handling", "Could be optimized further"],
            "reasoning": "The solution is well-organized and uses an efficient approach, but it does not account for potential edge cases and could be further optimized for larger inputs.",
            "score": 8.5
        }}

        """

        self._client.reset_context()
        self._client.append_message(UserMessage(eval_prompt))
        self._client.append_message(AssistantMessage("```json"))
        eval_text = self._client.get_response(stop_sequences=["```"])
        return json.loads(eval_text)

    def validateJson(self, json_str: str) -> float:
        try:
            json.loads(json_str)
            return 10
        except json.JSONDecodeError:
            return 0

    def validatePython(self, code_str: str) -> float:
        try:
            compile(code_str, "<string>", "exec")
            return 10
        except SyntaxError:
            return 0

    def validateRegex(self, regex_str: str) -> float:
        try:
            re.compile(regex_str)
            return 10
        except re.error:
            return 0

    def score_by_syntax(self, testCase: dict[str, str], response: str) -> float | None:
        match testCase.get("format"):
            case "json":
                return self.validateJson(response)
            case "python":
                return self.validatePython(response)
            case "regex":
                return self.validateRegex(response)
            case None:
                return None
            case unknown:
                raise ValueError(f"Unknown format: {unknown!r}")
