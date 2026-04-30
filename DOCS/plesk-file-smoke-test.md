# Plesk file smoke test

This is the first safe CI/CD step. It does not build or deploy the app. It only writes or deletes one file named `test.txt` in a test folder on Plesk.

## GitHub Secrets

Add these in GitHub repository settings:

- `PLESK_FILE_HOST`: `ftp://pleskserver.hostingdunyam.net`
- `PLESK_FILE_USERNAME`: FTP/SFTP username
- `PLESK_FILE_PASSWORD`: FTP/SFTP password
- `PLESK_TEST_REMOTE_DIR`: `/tummenu.com/webapp/ci-cd-smoke-test`

Optional:

- `PLESK_FTP_SSL_FORCE`: defaults to `true`
- `PLESK_FTP_SSL_VERIFY`: defaults to `true`

If the FTP certificate is self-signed, set `PLESK_FTP_SSL_VERIFY` to `false`.

## Test flow

1. Run `Plesk file smoke test` manually from GitHub Actions.
2. Choose `write` and set content like `first test`.
3. Confirm in Plesk File Manager that `test.txt` exists under `PLESK_TEST_REMOTE_DIR`.
4. Run it again with `write` and different content to test editing.
5. Run it with `delete` to remove only `test.txt`.

The real deploy workflow should only be added after this file-level smoke test passes.

Verified manually on April 30, 2026: FTP listing worked, `test.txt` was written, edited, read back, and deleted under `/tummenu.com/webapp/ci-cd-smoke-test`.

## Next gated test

`Plesk WebUI build test` is the next manual workflow. It builds, optionally tests, publishes `WebUI`, and uploads the publish output as a GitHub workflow artifact for inspection. It does not deploy app files.

If `deploy_marker` is checked, it uploads only one file named `ci-cd-build-marker.txt` to the WebUI remote folder. Add this secret before using that option:

- `PLESK_WEBAPP_REMOTE_DIR`: `/tummenu.com/webapp`

Keep `run_tests` enabled for CI confidence. It can be disabled only when testing publish artifact generation while existing tests are known to be failing.
