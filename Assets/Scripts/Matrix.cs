using System;
using System.Collections.Generic;

public static class Matrix {
	public static List<List<float>> Transpose(List<List<float>> matrix) {
		// MxN -> NxM
		List<List<float>> transposed = new List<List<float>>(matrix[0].Count);
		for (int i = 0; i < matrix[0].Count; ++i) {
			transposed.Add(new List<float>(matrix.Count));
			for (int j = 0; j < matrix.Count; ++j) {
				transposed[i].Add(matrix[j][i]);
			}
		}
		return transposed;
	}

	public static List<List<float>> Mul(List<List<float>> m1, List<List<float>> m2) {
		int aRows = m1.Count;
		int aCols = m1[0].Count;
		int bCols = m2[0].Count;

		// Optional: safety check
		if (m2.Count != aCols)
			throw new System.Exception($"Matrix dimensions do not match for multiplication: { m2.Count } vs { aCols }");

		// Create result matrix filled with 0s
		var result = new List<List<float>>(aRows);
		for (int i = 0; i < aRows; i++) {
			result.Add(new List<float>(new float[bCols]));
		}

		// Multiply
		for (int i = 0; i < aRows; i++) {
			for (int j = 0; j < bCols; j++) {
				float sum = 0f;

				for (int k = 0; k < aCols; k++) {
					sum += m1[i][k] * m2[k][j];
				}

				result[i][j] = sum;
			}
		}

		return result;
	}

	public static List<List<float>> Mul(List<List<float>> matrix, float num) {
		for (int i = 0; i < matrix.Count; ++i) {
			for (int j = 0; j < matrix[0].Count; ++j) {
				matrix[i][j] *= num;
			}
		}

		return matrix;
	}

	public static List<List<float>> Inverse(List<List<float>> matrix) {
		int n = matrix.Count;

		// Check square
		for (int i = 0; i < n; i++) {
			if (matrix[i].Count != n)
				throw new Exception("Matrix must be square");
		}

		// Create augmented matrix [A | I]
		float[,] aug = new float[n, 2 * n];

		for (int i = 0; i < n; i++) {
			for (int j = 0; j < n; j++) {
				aug[i, j] = matrix[i][j];
			}
			aug[i, i + n] = 1f;
		}

		// Gauss-Jordan elimination
		for (int i = 0; i < n; i++) {
			// Find pivot
			float pivot = aug[i, i];
			if (MathF.Abs(pivot) < 1e-8f)
				throw new Exception("Matrix is singular (not invertible): pivot = " + pivot);

			// Normalize row
			for (int j = 0; j < 2 * n; j++) {
				aug[i, j] /= pivot;
			}

			// Eliminate other rows
			for (int r = 0; r < n; r++) {
				if (r == i) continue;

				float factor = aug[r, i];

				for (int c = 0; c < 2 * n; c++) {
					aug[r, c] -= factor * aug[i, c];
				}
			}
		}

		// Extract inverse
		var result = new List<List<float>>(n);
		for (int i = 0; i < n; i++) {
			var row = new List<float>(n);
			for (int j = 0; j < n; j++) {
				row.Add(aug[i, j + n]);
			}
			result.Add(row);
		}

		return result;
	}

	public static List<List<float>> Add(List<List<float>> a, List<List<float>> b) {
		if (a == null || b == null)
			throw new ArgumentNullException("Input matrices cannot be null.");

		if (a.Count != b.Count)
			throw new ArgumentException($"Matrices must have the same number of rows. Comparing { a.Count } to { b.Count }");

		List<List<float>> result = new List<List<float>>();

		for (int i = 0; i < a.Count; i++) {
			if (a[i].Count != b[i].Count)
				throw new ArgumentException($"Row {i} does not have matching column sizes.");

			List<float> row = new List<float>();

			for (int j = 0; j < a[i].Count; j++) {
				row.Add(a[i][j] + b[i][j]);
			}

			result.Add(row);
		}

		return result;
	}

	public static float Length(List<List<float>> a) {
		float result = 0f;
		foreach (List<float> b in a) {
			result += b[0] * b[0];
		}

		return MathF.Sqrt(result);
	}
}