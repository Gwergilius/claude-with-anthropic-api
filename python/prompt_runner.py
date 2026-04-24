"""PromptRunner: runs prompt evaluation test cases through Claude."""

from __future__ import annotations

from anthropic_client import AnthropicClient
from anthropic_message import AssistantMessage, UserMessage
from grader import Grader


class PromptRunner:
    """Runs a single test case (prompt + task) through Claude and scores the response."""

    def __init__(self, testCases: list[dict[str, str]], client: AnthropicClient):
        self._testCases = testCases
        self._client = client
        self._grader = Grader(client)

    def runPrompt(
        self, testCase: dict[str, str], subject: str, placeholder: str = "{task}"
    ) -> str:
        """Merges the prompt and test case input, then returns its result.

        Args:
            testCase:     The test case dictionary containing the task ID and task text.
            subject:      The prompt template to evaluate.
            placeholder:  Token in subject to replace with the task text.
        """
        task = testCase.get("task")
        if task is None:
            raise ValueError(f"Task not found in test case: {testCase}")

        self._client.reset_context()
        prompt = subject.replace(placeholder, task)
        self._client.append_message(UserMessage(prompt))
        self._client.append_message(
            AssistantMessage(f"```{testCase.get('format', 'text')}")
        )
        response = self._client.get_response(stop_sequences=["```"])
        return response

    def runTestCase(
        self, testCase: dict[str, str], subject: str, placeholder: str = "{task}"
    ) -> dict:
        """Runs a single test case and returns the response along with its score.

        Args:
            testCase:     The test case dictionary containing the task ID and task text.
            subject:      The prompt template to evaluate.
            placeholder:  Token in subject to replace with the task text.
        Returns:
            A dictionary containing the response and its score.
        """
        response = self.runPrompt(testCase, subject)
        score = self._grader.scoreByModel(testCase, response)
        score["score"] += (
            s
            if (s := self._grader.score_by_syntax(testCase, response)) is not None
            else score["score"]
        )
        score["score"] /= 2

        return {
            "test_case": testCase,
            "output": response,
            "score": score,
        }

    def runEval(self, subject: str) -> list[dict[str, str | float]]:
        """Runs all test cases in the evaluation dataset and returns their responses and scores."""
        results = []
        for testCase in self._testCases:
            result = self.runTestCase(testCase, subject)
            results.append(result)
        return results
