# IAS Working Papers Redesign Package

**Status:** Proposed for review and approval

**Date:** 18 September 2026

**Baseline:** `AIS/Docs/reviews/LEGACY_WORKING_PAPERS_REVIEW.md`

This package defines a unified professional design for the five IAS working papers without changing application code, database objects, or existing workflows.

## Documents

1. [Unified architecture and workflow](UNIFIED_ARCHITECTURE.md) - common audit framework, roles, lifecycle, interface, controls, logical services, and acceptance criteria.
2. [Detailed working-paper layouts](WORKING_PAPER_LAYOUTS.md) - objectives, risks, procedures, fields, calculations, evidence, exceptions, and conclusions for Loan Case File, Voucher Checking, Account Opening, Fixed Assets, and Cash Count.
3. [Database impact and implementation plan](DATABASE_IMPACT_AND_IMPLEMENTATION.md) - proposed logical data model, legacy-record preservation, migration strategy, delivery phases, testing, rollout, and governance decisions.

## Design principles

- One professional audit lifecycle, five specialized test sheets.
- Evidence before assertion: every test result can be traced to procedure, evidence, exception, conclusion, preparer, reviewer, and version.
- Server-derived engagement and authorization context; never trust a query-string engagement ID by itself.
- Typed source values and server-side calculations; derived values are read-only.
- Maker-checker separation, controlled returns, explicit approvals, and immutable history.
- Existing records remain available and unchanged; no destructive migration or silent reinterpretation.
- Configuration should support methodology changes without turning the test sheets into unstructured forms.

## Approval gate

This package is a design proposal. Implementation must not start until Internal Audit methodology owners, application owners, Information Security, database administration, and records-management stakeholders approve the scope, data model, workflow, retention rules, and legacy treatment.
