// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/json.dart';

class RemoteServiceErrorResponse {
  const RemoteServiceErrorResponse({
    required this.error,
  });

  final RemoteServiceErrorInfo error;

  factory RemoteServiceErrorResponse.fromJson(Map<String, dynamic> json) => RemoteServiceErrorResponse(
        error: RemoteServiceErrorInfo.fromJson((json['error']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'error': error.toJson(),
      };
}

class RemoteServiceErrorInfo {
  const RemoteServiceErrorInfo({
    this.code,
    required this.message,
    this.details,
    this.data,
    this.validationErrors,
  });

  /// Stable machine code. Clients branch on this, never on message.
  final String? code;
  /// Localized according to Accept-Language.
  final String message;
  final String? details;
  final Map<String, dynamic>? data;
  final List<RemoteServiceValidationErrorInfo>? validationErrors;

  factory RemoteServiceErrorInfo.fromJson(Map<String, dynamic> json) => RemoteServiceErrorInfo(
        code: json['code'] == null ? null : (json['code']) as String,
        message: (json['message']) as String,
        details: json['details'] == null ? null : (json['details']) as String,
        data: json['data'] == null ? null : ((json['data']) as Map).cast<String, dynamic>(),
        validationErrors: json['validationErrors'] == null ? null : ((json['validationErrors']) as List).map((e) => RemoteServiceValidationErrorInfo.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (code != null) 'code': code!,
        'message': message,
        if (details != null) 'details': details!,
        if (data != null) 'data': data!,
        if (validationErrors != null) 'validationErrors': validationErrors!.map((e) => e.toJson()).toList(),
      };
}

class RemoteServiceValidationErrorInfo {
  const RemoteServiceValidationErrorInfo({
    required this.message,
    required this.members,
  });

  final String message;
  final List<String> members;

  factory RemoteServiceValidationErrorInfo.fromJson(Map<String, dynamic> json) => RemoteServiceValidationErrorInfo(
        message: (json['message']) as String,
        members: ((json['members']) as List).map((e) => (e) as String).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'message': message,
        'members': members,
      };
}

enum AppRole {
  employee('employee'),
  departmentManager('departmentManager'),
  hrManager('hrManager'),
  executiveDirector('executiveDirector'),
  reviewer('reviewer'),
  accountant('accountant'),
  storeOfficer('storeOfficer'),
  systemAdmin('systemAdmin'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AppRole(this.value);
  final String value;

  static AppRole fromJson(Object? json) {
    for (final v in AppRole.values) {
      if (v.value == json) return v;
    }
    return AppRole.$unknown;
  }

  String toJson() => value;
}

/// Minimal employee reference. Safe for store officer lookup. Never add other fields.
class EmployeeRefDto {
  const EmployeeRefDto({
    required this.id,
    required this.fullName,
    this.departmentName,
  });

  final String id;
  final String fullName;
  /// Localized by Accept-Language.
  final String? departmentName;

  factory EmployeeRefDto.fromJson(Map<String, dynamic> json) => EmployeeRefDto(
        id: (json['id']) as String,
        fullName: (json['fullName']) as String,
        departmentName: json['departmentName'] == null ? null : (json['departmentName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'fullName': fullName,
        if (departmentName != null) 'departmentName': departmentName!,
      };
}

class UserRefDto {
  const UserRefDto({
    required this.id,
    required this.displayName,
  });

  final String id;
  final String displayName;

  factory UserRefDto.fromJson(Map<String, dynamic> json) => UserRefDto(
        id: (json['id']) as String,
        displayName: (json['displayName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'displayName': displayName,
      };
}

class DepartmentRefDto {
  const DepartmentRefDto({
    required this.id,
    required this.name,
  });

  final String id;
  /// Localized by Accept-Language.
  final String name;

  factory DepartmentRefDto.fromJson(Map<String, dynamic> json) => DepartmentRefDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
      };
}

class AttachmentDto {
  const AttachmentDto({
    required this.id,
    required this.fileName,
    required this.contentType,
    required this.sizeBytes,
    required this.creationTime,
  });

  final String id;
  final String fileName;
  final String contentType;
  final int sizeBytes;
  final DateTime creationTime;

  factory AttachmentDto.fromJson(Map<String, dynamic> json) => AttachmentDto(
        id: (json['id']) as String,
        fileName: (json['fileName']) as String,
        contentType: (json['contentType']) as String,
        sizeBytes: ((json['sizeBytes']) as num).toInt(),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'fileName': fileName,
        'contentType': contentType,
        'sizeBytes': sizeBytes,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class ReasonInputDto {
  const ReasonInputDto({
    required this.reason,
  });

  final String reason;

  factory ReasonInputDto.fromJson(Map<String, dynamic> json) => ReasonInputDto(
        reason: (json['reason']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'reason': reason,
      };
}

class NoteInputDto {
  const NoteInputDto({
    this.note,
  });

  final String? note;

  factory NoteInputDto.fromJson(Map<String, dynamic> json) => NoteInputDto(
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (note != null) 'note': note!,
      };
}

class FileDownloadUrlDto {
  const FileDownloadUrlDto({
    required this.url,
    required this.expiresAt,
  });

  /// Short-lived download link.
  final String url;
  final DateTime expiresAt;

  factory FileDownloadUrlDto.fromJson(Map<String, dynamic> json) => FileDownloadUrlDto(
        url: (json['url']) as String,
        expiresAt: DateTime.parse((json['expiresAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'url': url,
        'expiresAt': hrDateTimeToJson(expiresAt),
      };
}

/// One decision or event in a request timeline.
class TimelineEntryDto {
  const TimelineEntryDto({
    required this.occurredAt,
    required this.action,
    this.actor,
    this.note,
  });

  final DateTime occurredAt;
  final String action;
  final UserRefDto? actor;
  final String? note;

  factory TimelineEntryDto.fromJson(Map<String, dynamic> json) => TimelineEntryDto(
        occurredAt: DateTime.parse((json['occurredAt']) as String),
        action: (json['action']) as String,
        actor: json['actor'] == null ? null : UserRefDto.fromJson((json['actor']) as Map<String, dynamic>),
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'occurredAt': hrDateTimeToJson(occurredAt),
        'action': action,
        if (actor != null) 'actor': actor!.toJson(),
        if (note != null) 'note': note!,
      };
}

/// OpenID Connect discovery document. Property names are snake_case as fixed by the standard; this is an intended exception to camelCase.
class OpenIdConfigurationDto {
  const OpenIdConfigurationDto({
    required this.issuer,
    required this.authorizationEndpoint,
    required this.tokenEndpoint,
    this.endSessionEndpoint,
    this.userinfoEndpoint,
    this.revocationEndpoint,
    required this.jwksUri,
    this.scopesSupported,
    required this.responseTypesSupported,
    required this.grantTypesSupported,
    required this.codeChallengeMethodsSupported,
  });

  final String issuer;
  final String authorizationEndpoint;
  final String tokenEndpoint;
  final String? endSessionEndpoint;
  final String? userinfoEndpoint;
  final String? revocationEndpoint;
  final String jwksUri;
  final List<String>? scopesSupported;
  final List<String> responseTypesSupported;
  final List<String> grantTypesSupported;
  final List<String> codeChallengeMethodsSupported;

  factory OpenIdConfigurationDto.fromJson(Map<String, dynamic> json) => OpenIdConfigurationDto(
        issuer: (json['issuer']) as String,
        authorizationEndpoint: (json['authorization_endpoint']) as String,
        tokenEndpoint: (json['token_endpoint']) as String,
        endSessionEndpoint: json['end_session_endpoint'] == null ? null : (json['end_session_endpoint']) as String,
        userinfoEndpoint: json['userinfo_endpoint'] == null ? null : (json['userinfo_endpoint']) as String,
        revocationEndpoint: json['revocation_endpoint'] == null ? null : (json['revocation_endpoint']) as String,
        jwksUri: (json['jwks_uri']) as String,
        scopesSupported: json['scopes_supported'] == null ? null : ((json['scopes_supported']) as List).map((e) => (e) as String).toList(),
        responseTypesSupported: ((json['response_types_supported']) as List).map((e) => (e) as String).toList(),
        grantTypesSupported: ((json['grant_types_supported']) as List).map((e) => (e) as String).toList(),
        codeChallengeMethodsSupported: ((json['code_challenge_methods_supported']) as List).map((e) => (e) as String).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'issuer': issuer,
        'authorization_endpoint': authorizationEndpoint,
        'token_endpoint': tokenEndpoint,
        if (endSessionEndpoint != null) 'end_session_endpoint': endSessionEndpoint!,
        if (userinfoEndpoint != null) 'userinfo_endpoint': userinfoEndpoint!,
        if (revocationEndpoint != null) 'revocation_endpoint': revocationEndpoint!,
        'jwks_uri': jwksUri,
        if (scopesSupported != null) 'scopes_supported': scopesSupported!,
        'response_types_supported': responseTypesSupported,
        'grant_types_supported': grantTypesSupported,
        'code_challenge_methods_supported': codeChallengeMethodsSupported,
      };
}

/// Form body of POST /connect/token. Property names are snake_case as fixed by OAuth 2.0.
class ConnectTokenRequestDto {
  const ConnectTokenRequestDto({
    required this.grantType,
    required this.clientId,
    this.code,
    this.redirectUri,
    this.codeVerifier,
    this.refreshToken,
    this.scope,
  });

  /// The password grant is not supported.
  final String grantType;
  /// Public client id of the Flutter app. No client secret exists.
  final String clientId;
  /// Required when grant_type is authorization_code.
  final String? code;
  /// Required when grant_type is authorization_code. Must equal the value sent to /connect/authorize.
  final String? redirectUri;
  /// PKCE verifier. Required when grant_type is authorization_code.
  final String? codeVerifier;
  /// Required when grant_type is refresh_token.
  final String? refreshToken;
  /// Space separated scopes, for example: openid profile offline_access HR_Mangment2.
  final String? scope;

  factory ConnectTokenRequestDto.fromJson(Map<String, dynamic> json) => ConnectTokenRequestDto(
        grantType: (json['grant_type']) as String,
        clientId: (json['client_id']) as String,
        code: json['code'] == null ? null : (json['code']) as String,
        redirectUri: json['redirect_uri'] == null ? null : (json['redirect_uri']) as String,
        codeVerifier: json['code_verifier'] == null ? null : (json['code_verifier']) as String,
        refreshToken: json['refresh_token'] == null ? null : (json['refresh_token']) as String,
        scope: json['scope'] == null ? null : (json['scope']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'grant_type': grantType,
        'client_id': clientId,
        if (code != null) 'code': code!,
        if (redirectUri != null) 'redirect_uri': redirectUri!,
        if (codeVerifier != null) 'code_verifier': codeVerifier!,
        if (refreshToken != null) 'refresh_token': refreshToken!,
        if (scope != null) 'scope': scope!,
      };
}

/// OAuth 2.0 token response. Property names are snake_case as fixed by the standard.
class ConnectTokenResponseDto {
  const ConnectTokenResponseDto({
    required this.accessToken,
    required this.tokenType,
    required this.expiresIn,
    this.refreshToken,
    this.idToken,
    this.scope,
  });

  final String accessToken;
  final String tokenType;
  /// Access token lifetime in seconds.
  final int expiresIn;
  /// Present when the offline_access scope was granted.
  final String? refreshToken;
  /// Present when the openid scope was granted.
  final String? idToken;
  final String? scope;

  factory ConnectTokenResponseDto.fromJson(Map<String, dynamic> json) => ConnectTokenResponseDto(
        accessToken: (json['access_token']) as String,
        tokenType: (json['token_type']) as String,
        expiresIn: ((json['expires_in']) as num).toInt(),
        refreshToken: json['refresh_token'] == null ? null : (json['refresh_token']) as String,
        idToken: json['id_token'] == null ? null : (json['id_token']) as String,
        scope: json['scope'] == null ? null : (json['scope']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'access_token': accessToken,
        'token_type': tokenType,
        'expires_in': expiresIn,
        if (refreshToken != null) 'refresh_token': refreshToken!,
        if (idToken != null) 'id_token': idToken!,
        if (scope != null) 'scope': scope!,
      };
}

/// OAuth 2.0 error body. /connect/token is the only route in this module that does not use RemoteServiceErrorResponse.
class ConnectTokenErrorDto {
  const ConnectTokenErrorDto({
    required this.error,
    this.errorDescription,
  });

  /// OAuth 2.0 error code. A suspended or ended account gets invalid_grant.
  final String error;
  final String? errorDescription;

  factory ConnectTokenErrorDto.fromJson(Map<String, dynamic> json) => ConnectTokenErrorDto(
        error: (json['error']) as String,
        errorDescription: json['error_description'] == null ? null : (json['error_description']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'error': error,
        if (errorDescription != null) 'error_description': errorDescription!,
      };
}

/// The fields of the ABP application configuration that the Flutter app relies on. ABP may return other fields; they are not part of this contract and the client must ignore them.
class ApplicationConfigurationDto {
  const ApplicationConfigurationDto({
    required this.localization,
    required this.auth,
    required this.setting,
    required this.currentUser,
    this.extraProperties,
  });

  final AppConfigLocalizationDto localization;
  final AppConfigAuthDto auth;
  final AppConfigSettingDto setting;
  final AppConfigCurrentUserDto currentUser;
  /// Added by a backend IApplicationConfigurationContributor. Null for anonymous calls.
  final AppConfigExtraPropertiesDto? extraProperties;

  factory ApplicationConfigurationDto.fromJson(Map<String, dynamic> json) => ApplicationConfigurationDto(
        localization: AppConfigLocalizationDto.fromJson((json['localization']) as Map<String, dynamic>),
        auth: AppConfigAuthDto.fromJson((json['auth']) as Map<String, dynamic>),
        setting: AppConfigSettingDto.fromJson((json['setting']) as Map<String, dynamic>),
        currentUser: AppConfigCurrentUserDto.fromJson((json['currentUser']) as Map<String, dynamic>),
        extraProperties: json['extraProperties'] == null ? null : AppConfigExtraPropertiesDto.fromJson((json['extraProperties']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'localization': localization.toJson(),
        'auth': auth.toJson(),
        'setting': setting.toJson(),
        'currentUser': currentUser.toJson(),
        if (extraProperties != null) 'extraProperties': extraProperties!.toJson(),
      };
}

class AppConfigLocalizationDto {
  const AppConfigLocalizationDto({
    required this.currentCulture,
    this.defaultResourceName,
    required this.languages,
    required this.values,
  });

  final AppConfigCurrentCultureDto currentCulture;
  final String? defaultResourceName;
  /// Exactly ar and en. ar is the default.
  final List<AppConfigLanguageDto> languages;
  /// Backend localization texts: resource name, then key, then text. The Flutter UI uses its own ARB files for labels.
  final Map<String, Map<String, String>> values;

  factory AppConfigLocalizationDto.fromJson(Map<String, dynamic> json) => AppConfigLocalizationDto(
        currentCulture: AppConfigCurrentCultureDto.fromJson((json['currentCulture']) as Map<String, dynamic>),
        defaultResourceName: json['defaultResourceName'] == null ? null : (json['defaultResourceName']) as String,
        languages: ((json['languages']) as List).map((e) => AppConfigLanguageDto.fromJson((e) as Map<String, dynamic>)).toList(),
        values: ((json['values']) as Map).map((k, v) => MapEntry(k as String, ((v) as Map).map((k, v) => MapEntry(k as String, (v) as String)))),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'currentCulture': currentCulture.toJson(),
        if (defaultResourceName != null) 'defaultResourceName': defaultResourceName!,
        'languages': languages.map((e) => e.toJson()).toList(),
        'values': values,
      };
}

class AppConfigCurrentCultureDto {
  const AppConfigCurrentCultureDto({
    required this.cultureName,
    required this.name,
    required this.displayName,
    required this.twoLetterIsoLanguageName,
    required this.isRightToLeft,
  });

  final String cultureName;
  final String name;
  final String displayName;
  final String twoLetterIsoLanguageName;
  /// Drives the screen direction.
  final bool isRightToLeft;

  factory AppConfigCurrentCultureDto.fromJson(Map<String, dynamic> json) => AppConfigCurrentCultureDto(
        cultureName: (json['cultureName']) as String,
        name: (json['name']) as String,
        displayName: (json['displayName']) as String,
        twoLetterIsoLanguageName: (json['twoLetterIsoLanguageName']) as String,
        isRightToLeft: (json['isRightToLeft']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'cultureName': cultureName,
        'name': name,
        'displayName': displayName,
        'twoLetterIsoLanguageName': twoLetterIsoLanguageName,
        'isRightToLeft': isRightToLeft,
      };
}

class AppConfigLanguageDto {
  const AppConfigLanguageDto({
    required this.cultureName,
    required this.uiCultureName,
    required this.displayName,
  });

  final String cultureName;
  final String uiCultureName;
  final String displayName;

  factory AppConfigLanguageDto.fromJson(Map<String, dynamic> json) => AppConfigLanguageDto(
        cultureName: (json['cultureName']) as String,
        uiCultureName: (json['uiCultureName']) as String,
        displayName: (json['displayName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'cultureName': cultureName,
        'uiCultureName': uiCultureName,
        'displayName': displayName,
      };
}

class AppConfigAuthDto {
  const AppConfigAuthDto({
    required this.grantedPolicies,
  });

  /// Permission names granted to the current user (union of all roles and direct grants). Only granted names appear, each with the value true. Used to hide UI; the server still checks every call.
  final Map<String, bool> grantedPolicies;

  factory AppConfigAuthDto.fromJson(Map<String, dynamic> json) => AppConfigAuthDto(
        grantedPolicies: ((json['grantedPolicies']) as Map).map((k, v) => MapEntry(k as String, (v) as bool)),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'grantedPolicies': grantedPolicies,
      };
}

class AppConfigSettingDto {
  const AppConfigSettingDto({
    required this.values,
  });

  /// Settings visible to this client. Never contains secrets. The backend must expose Abp.Localization.DefaultLanguage and the Abp.Identity.Password.* rules (RequiredLength, RequiredUniqueChars, RequireNonAlphanumeric, RequireLowercase, RequireUppercase, RequireDigit), and Hr.Document.MaxFileSizeBytes and Hr.Document.AllowedContentTypes.
  final Map<String, String?> values;

  factory AppConfigSettingDto.fromJson(Map<String, dynamic> json) => AppConfigSettingDto(
        values: ((json['values']) as Map).map((k, v) => MapEntry(k as String, v == null ? null : (v) as String)),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'values': values,
      };
}

class AppConfigCurrentUserDto {
  const AppConfigCurrentUserDto({
    required this.isAuthenticated,
    this.id,
    this.tenantId,
    this.userName,
    this.name,
    this.surName,
    this.email,
    this.emailVerified,
    this.phoneNumber,
    this.phoneNumberVerified,
    required this.roles,
  });

  final bool isAuthenticated;
  /// User id. Null when anonymous.
  final String? id;
  final String? tenantId;
  final String? userName;
  final String? name;
  final String? surName;
  final String? email;
  final bool? emailVerified;
  final String? phoneNumber;
  final bool? phoneNumberVerified;
  /// Role names. Built-in roles use AppRole values. A user may hold several, and permissions are their union.
  final List<String> roles;

  factory AppConfigCurrentUserDto.fromJson(Map<String, dynamic> json) => AppConfigCurrentUserDto(
        isAuthenticated: (json['isAuthenticated']) as bool,
        id: json['id'] == null ? null : (json['id']) as String,
        tenantId: json['tenantId'] == null ? null : (json['tenantId']) as String,
        userName: json['userName'] == null ? null : (json['userName']) as String,
        name: json['name'] == null ? null : (json['name']) as String,
        surName: json['surName'] == null ? null : (json['surName']) as String,
        email: json['email'] == null ? null : (json['email']) as String,
        emailVerified: json['emailVerified'] == null ? null : (json['emailVerified']) as bool,
        phoneNumber: json['phoneNumber'] == null ? null : (json['phoneNumber']) as String,
        phoneNumberVerified: json['phoneNumberVerified'] == null ? null : (json['phoneNumberVerified']) as bool,
        roles: ((json['roles']) as List).map((e) => (e) as String).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'isAuthenticated': isAuthenticated,
        if (id != null) 'id': id!,
        if (tenantId != null) 'tenantId': tenantId!,
        if (userName != null) 'userName': userName!,
        if (name != null) 'name': name!,
        if (surName != null) 'surName': surName!,
        if (email != null) 'email': email!,
        if (emailVerified != null) 'emailVerified': emailVerified!,
        if (phoneNumber != null) 'phoneNumber': phoneNumber!,
        if (phoneNumberVerified != null) 'phoneNumberVerified': phoneNumberVerified!,
        'roles': roles,
      };
}

/// Same values as MyAccountDto, so the client can route on startup with one call.
class AppConfigExtraPropertiesDto {
  const AppConfigExtraPropertiesDto({
    required this.accessGate,
    required this.mustChangePassword,
    required this.mfaRequired,
    required this.mfaEnabled,
  });

  final AccessGate accessGate;
  final bool mustChangePassword;
  final bool mfaRequired;
  final bool mfaEnabled;

  factory AppConfigExtraPropertiesDto.fromJson(Map<String, dynamic> json) => AppConfigExtraPropertiesDto(
        accessGate: AccessGate.fromJson(json['accessGate']),
        mustChangePassword: (json['mustChangePassword']) as bool,
        mfaRequired: (json['mfaRequired']) as bool,
        mfaEnabled: (json['mfaEnabled']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'accessGate': accessGate.toJson(),
        'mustChangePassword': mustChangePassword,
        'mfaRequired': mfaRequired,
        'mfaEnabled': mfaEnabled,
      };
}

/// What the user must finish before using the system. passwordChangeRequired is evaluated first. Any value other than none blocks other operations with 403; see x-global-rules.access-gates.
enum AccessGate {
  none('none'),
  passwordChangeRequired('passwordChangeRequired'),
  mfaEnrollmentRequired('mfaEnrollmentRequired'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AccessGate(this.value);
  final String value;

  static AccessGate fromJson(Object? json) {
    for (final v in AccessGate.values) {
      if (v.value == json) return v;
    }
    return AccessGate.$unknown;
  }

  String toJson() => value;
}

enum AppLanguage {
  ar('ar'),
  en('en'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AppLanguage(this.value);
  final String value;

  static AppLanguage fromJson(Object? json) {
    for (final v in AppLanguage.values) {
      if (v.value == json) return v;
    }
    return AppLanguage.$unknown;
  }

  String toJson() => value;
}

class MyAccountDto {
  const MyAccountDto({
    required this.userId,
    required this.userName,
    required this.displayName,
    this.email,
    required this.preferredLanguage,
    required this.roleNames,
    this.employee,
    required this.directReportCount,
    required this.accessGate,
    required this.mustChangePassword,
    required this.mfaRequired,
    required this.mfaEnabled,
  });

  final String userId;
  final String userName;
  final String displayName;
  final String? email;
  final AppLanguage preferredLanguage;
  final List<String> roleNames;
  /// Null only for a technical account that has no employee file.
  final EmployeeRefDto? employee;
  /// Employees currently linked to this user as their manager.
  final int directReportCount;
  final AccessGate accessGate;
  final bool mustChangePassword;
  /// True when the user holds hrManager, executiveDirector, accountant or systemAdmin.
  final bool mfaRequired;
  final bool mfaEnabled;

  factory MyAccountDto.fromJson(Map<String, dynamic> json) => MyAccountDto(
        userId: (json['userId']) as String,
        userName: (json['userName']) as String,
        displayName: (json['displayName']) as String,
        email: json['email'] == null ? null : (json['email']) as String,
        preferredLanguage: AppLanguage.fromJson(json['preferredLanguage']),
        roleNames: ((json['roleNames']) as List).map((e) => (e) as String).toList(),
        employee: json['employee'] == null ? null : EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        directReportCount: ((json['directReportCount']) as num).toInt(),
        accessGate: AccessGate.fromJson(json['accessGate']),
        mustChangePassword: (json['mustChangePassword']) as bool,
        mfaRequired: (json['mfaRequired']) as bool,
        mfaEnabled: (json['mfaEnabled']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'userId': userId,
        'userName': userName,
        'displayName': displayName,
        if (email != null) 'email': email!,
        'preferredLanguage': preferredLanguage.toJson(),
        'roleNames': roleNames,
        if (employee != null) 'employee': employee!.toJson(),
        'directReportCount': directReportCount,
        'accessGate': accessGate.toJson(),
        'mustChangePassword': mustChangePassword,
        'mfaRequired': mfaRequired,
        'mfaEnabled': mfaEnabled,
      };
}

class MyAccountUpdateDto {
  const MyAccountUpdateDto({
    required this.preferredLanguage,
  });

  final AppLanguage preferredLanguage;

  factory MyAccountUpdateDto.fromJson(Map<String, dynamic> json) => MyAccountUpdateDto(
        preferredLanguage: AppLanguage.fromJson(json['preferredLanguage']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'preferredLanguage': preferredLanguage.toJson(),
      };
}

class ChangePasswordInputDto {
  const ChangePasswordInputDto({
    required this.currentPassword,
    required this.newPassword,
  });

  /// The current or temporary password.
  final String currentPassword;
  /// Must satisfy the Abp.Identity.Password.* settings.
  final String newPassword;

  factory ChangePasswordInputDto.fromJson(Map<String, dynamic> json) => ChangePasswordInputDto(
        currentPassword: (json['currentPassword']) as String,
        newPassword: (json['newPassword']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'currentPassword': currentPassword,
        'newPassword': newPassword,
      };
}

/// accessGate tells the client the next step, for example mfaEnrollmentRequired.
class ChangePasswordResultDto {
  const ChangePasswordResultDto({
    required this.accessGate,
    required this.changedAt,
  });

  final AccessGate accessGate;
  final DateTime changedAt;

  factory ChangePasswordResultDto.fromJson(Map<String, dynamic> json) => ChangePasswordResultDto(
        accessGate: AccessGate.fromJson(json['accessGate']),
        changedAt: DateTime.parse((json['changedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'accessGate': accessGate.toJson(),
        'changedAt': hrDateTimeToJson(changedAt),
      };
}

/// Kind of pending item. Complaints are deliberately not a category.
enum InboxCategory {
  request('request'),
  memo('memo'),
  incident('incident'),
  stockIssue('stockIssue'),
  evaluation('evaluation'),
  departmentReport('departmentReport'),
  evaluationObjection('evaluationObjection'),
  evaluationAcknowledgement('evaluationAcknowledgement'),
  circularAcknowledgement('circularAcknowledgement'),
  custodyAcknowledgement('custodyAcknowledgement'),
  warningResponse('warningResponse'),
  warningDecision('warningDecision'),
  contractDecision('contractDecision'),
  appreciationLetterAcknowledgement('appreciationLetterAcknowledgement'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const InboxCategory(this.value);
  final String value;

  static InboxCategory fromJson(Object? json) {
    for (final v in InboxCategory.values) {
      if (v.value == json) return v;
    }
    return InboxCategory.$unknown;
  }

  String toJson() => value;
}

class InboxSummaryItemDto {
  const InboxSummaryItemDto({
    required this.category,
    required this.count,
    required this.delegatedCount,
  });

  final InboxCategory category;
  /// All pending items in this category, including delegated ones.
  final int count;
  /// The part of count that reached this user through an active delegation.
  final int delegatedCount;

  factory InboxSummaryItemDto.fromJson(Map<String, dynamic> json) => InboxSummaryItemDto(
        category: InboxCategory.fromJson(json['category']),
        count: ((json['count']) as num).toInt(),
        delegatedCount: ((json['delegatedCount']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'category': category.toJson(),
        'count': count,
        'delegatedCount': delegatedCount,
      };
}

class InboxSummaryDto {
  const InboxSummaryDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  /// Only categories with a count above zero.
  final List<InboxSummaryItemDto> items;

  factory InboxSummaryDto.fromJson(Map<String, dynamic> json) => InboxSummaryDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => InboxSummaryItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

enum AdministrativeTaskType {
  requestApproved('requestApproved'),
  requestRejected('requestRejected'),
  requestSubmittedOnBehalf('requestSubmittedOnBehalf'),
  memoProcessed('memoProcessed'),
  incidentForwarded('incidentForwarded'),
  evaluationSubmitted('evaluationSubmitted'),
  departmentReportSubmitted('departmentReportSubmitted'),
  circularSent('circularSent'),
  custodyCountRecorded('custodyCountRecorded'),
  warningRecommended('warningRecommended'),
  delegationCreated('delegationCreated'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AdministrativeTaskType(this.value);
  final String value;

  static AdministrativeTaskType fromJson(Object? json) {
    for (final v in AdministrativeTaskType.values) {
      if (v.value == json) return v;
    }
    return AdministrativeTaskType.$unknown;
  }

  String toJson() => value;
}

/// One administrative task done by a manager. Complaint work never appears here.
class AdministrativeTaskLogEntryDto {
  const AdministrativeTaskLogEntryDto({
    required this.occurredAt,
    required this.taskType,
    required this.title,
    this.referenceId,
  });

  final DateTime occurredAt;
  final AdministrativeTaskType taskType;
  /// Localized by Accept-Language.
  final String title;
  /// Id of the related record; its kind follows from taskType.
  final String? referenceId;

  factory AdministrativeTaskLogEntryDto.fromJson(Map<String, dynamic> json) => AdministrativeTaskLogEntryDto(
        occurredAt: DateTime.parse((json['occurredAt']) as String),
        taskType: AdministrativeTaskType.fromJson(json['taskType']),
        title: (json['title']) as String,
        referenceId: json['referenceId'] == null ? null : (json['referenceId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'occurredAt': hrDateTimeToJson(occurredAt),
        'taskType': taskType.toJson(),
        'title': title,
        if (referenceId != null) 'referenceId': referenceId!,
      };
}

class AdministrativeTaskLogPagedResultDto {
  const AdministrativeTaskLogPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<AdministrativeTaskLogEntryDto> items;

  factory AdministrativeTaskLogPagedResultDto.fromJson(Map<String, dynamic> json) => AdministrativeTaskLogPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AdministrativeTaskLogEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class MfaStatusDto {
  const MfaStatusDto({
    required this.isRequired,
    required this.isEnabled,
    this.enabledAt,
    required this.recoveryCodesRemaining,
  });

  final bool isRequired;
  final bool isEnabled;
  final DateTime? enabledAt;
  final int recoveryCodesRemaining;

  factory MfaStatusDto.fromJson(Map<String, dynamic> json) => MfaStatusDto(
        isRequired: (json['isRequired']) as bool,
        isEnabled: (json['isEnabled']) as bool,
        enabledAt: json['enabledAt'] == null ? null : DateTime.parse((json['enabledAt']) as String),
        recoveryCodesRemaining: ((json['recoveryCodesRemaining']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'isRequired': isRequired,
        'isEnabled': isEnabled,
        if (enabledAt != null) 'enabledAt': hrDateTimeToJson(enabledAt!),
        'recoveryCodesRemaining': recoveryCodesRemaining,
      };
}

class MfaEnrollmentDto {
  const MfaEnrollmentDto({
    required this.sharedKey,
    required this.authenticatorUri,
  });

  /// TOTP secret in Base32, for manual entry. Returned only by this call.
  final String sharedKey;
  /// otpauth:// URI to render as a QR code.
  final String authenticatorUri;

  factory MfaEnrollmentDto.fromJson(Map<String, dynamic> json) => MfaEnrollmentDto(
        sharedKey: (json['sharedKey']) as String,
        authenticatorUri: (json['authenticatorUri']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'sharedKey': sharedKey,
        'authenticatorUri': authenticatorUri,
      };
}

class MfaCodeInputDto {
  const MfaCodeInputDto({
    required this.code,
  });

  /// Six-digit TOTP code.
  final String code;

  factory MfaCodeInputDto.fromJson(Map<String, dynamic> json) => MfaCodeInputDto(
        code: (json['code']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'code': code,
      };
}

class MfaRecoveryCodesDto {
  const MfaRecoveryCodesDto({
    required this.recoveryCodes,
    required this.accessGate,
  });

  /// Shown once only. Each code works one time at sign-in.
  final List<String> recoveryCodes;
  final AccessGate accessGate;

  factory MfaRecoveryCodesDto.fromJson(Map<String, dynamic> json) => MfaRecoveryCodesDto(
        recoveryCodes: ((json['recoveryCodes']) as List).map((e) => (e) as String).toList(),
        accessGate: AccessGate.fromJson(json['accessGate']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'recoveryCodes': recoveryCodes,
        'accessGate': accessGate.toJson(),
      };
}

/// Account as seen by HR and the system admin. No password value or hash exists in any DTO.
class UserAccountDto {
  const UserAccountDto({
    required this.id,
    required this.userName,
    this.email,
    this.employee,
    required this.isActive,
    required this.isLockedOut,
    required this.mustChangePassword,
    required this.mfaRequired,
    required this.mfaEnabled,
    required this.roleNames,
    required this.creationTime,
    this.lastModificationTime,
    required this.concurrencyStamp,
  });

  /// User id, not employee id.
  final String id;
  final String userName;
  final String? email;
  /// Null only for a technical account that has no employee file.
  final EmployeeRefDto? employee;
  /// False while the employee is suspended or ended.
  final bool isActive;
  /// Temporary sign-in lockout. Cleared by a password reset.
  final bool isLockedOut;
  final bool mustChangePassword;
  final bool mfaRequired;
  final bool mfaEnabled;
  final List<String> roleNames;
  final DateTime creationTime;
  final DateTime? lastModificationTime;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;

  factory UserAccountDto.fromJson(Map<String, dynamic> json) => UserAccountDto(
        id: (json['id']) as String,
        userName: (json['userName']) as String,
        email: json['email'] == null ? null : (json['email']) as String,
        employee: json['employee'] == null ? null : EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        isActive: (json['isActive']) as bool,
        isLockedOut: (json['isLockedOut']) as bool,
        mustChangePassword: (json['mustChangePassword']) as bool,
        mfaRequired: (json['mfaRequired']) as bool,
        mfaEnabled: (json['mfaEnabled']) as bool,
        roleNames: ((json['roleNames']) as List).map((e) => (e) as String).toList(),
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'userName': userName,
        if (email != null) 'email': email!,
        if (employee != null) 'employee': employee!.toJson(),
        'isActive': isActive,
        'isLockedOut': isLockedOut,
        'mustChangePassword': mustChangePassword,
        'mfaRequired': mfaRequired,
        'mfaEnabled': mfaEnabled,
        'roleNames': roleNames,
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
        'concurrencyStamp': concurrencyStamp,
      };
}

class UserAccountPagedResultDto {
  const UserAccountPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<UserAccountDto> items;

  factory UserAccountPagedResultDto.fromJson(Map<String, dynamic> json) => UserAccountPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => UserAccountDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class UserAccountCreateDto {
  const UserAccountCreateDto({
    required this.employeeId,
    required this.userName,
    this.email,
    required this.roleNames,
  });

  /// An employee that has no account yet.
  final String employeeId;
  final String userName;
  /// Defaults to the email on the employee file.
  final String? email;
  /// Role names. The server always adds employee.
  final List<String> roleNames;

  factory UserAccountCreateDto.fromJson(Map<String, dynamic> json) => UserAccountCreateDto(
        employeeId: (json['employeeId']) as String,
        userName: (json['userName']) as String,
        email: json['email'] == null ? null : (json['email']) as String,
        roleNames: ((json['roleNames']) as List).map((e) => (e) as String).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employeeId': employeeId,
        'userName': userName,
        if (email != null) 'email': email!,
        'roleNames': roleNames,
      };
}

class TemporaryCredentialsDto {
  const TemporaryCredentialsDto({
    required this.userId,
    required this.userName,
    required this.temporaryPassword,
    required this.mustChangePassword,
  });

  final String userId;
  final String userName;
  /// Generated by the server and returned only in this response. It cannot be read again by anyone; a lost value requires a reset.
  final String temporaryPassword;
  /// Always true.
  final bool mustChangePassword;

  factory TemporaryCredentialsDto.fromJson(Map<String, dynamic> json) => TemporaryCredentialsDto(
        userId: (json['userId']) as String,
        userName: (json['userName']) as String,
        temporaryPassword: (json['temporaryPassword']) as String,
        mustChangePassword: (json['mustChangePassword']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'userId': userId,
        'userName': userName,
        'temporaryPassword': temporaryPassword,
        'mustChangePassword': mustChangePassword,
      };
}

class UserAccountCreatedDto {
  const UserAccountCreatedDto({
    required this.account,
    required this.credentials,
  });

  final UserAccountDto account;
  final TemporaryCredentialsDto credentials;

  factory UserAccountCreatedDto.fromJson(Map<String, dynamic> json) => UserAccountCreatedDto(
        account: UserAccountDto.fromJson((json['account']) as Map<String, dynamic>),
        credentials: TemporaryCredentialsDto.fromJson((json['credentials']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'account': account.toJson(),
        'credentials': credentials.toJson(),
      };
}

class UserRolesUpdateDto {
  const UserRolesUpdateDto({
    required this.roleNames,
    this.managerEmployeeId,
    required this.concurrencyStamp,
  });

  /// The complete new set of roles. The server always keeps employee on accounts linked to an employee file.
  final List<String> roleNames;
  /// Needed only when the new roles make the current manager link invalid. Applied in the same transaction with the effects of a manager change.
  final String? managerEmployeeId;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;

  factory UserRolesUpdateDto.fromJson(Map<String, dynamic> json) => UserRolesUpdateDto(
        roleNames: ((json['roleNames']) as List).map((e) => (e) as String).toList(),
        managerEmployeeId: json['managerEmployeeId'] == null ? null : (json['managerEmployeeId']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'roleNames': roleNames,
        if (managerEmployeeId != null) 'managerEmployeeId': managerEmployeeId!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class RoleDto {
  const RoleDto({
    required this.id,
    required this.name,
    required this.displayName,
    required this.nameAr,
    required this.nameEn,
    required this.isStatic,
    required this.userCount,
    required this.concurrencyStamp,
  });

  final String id;
  /// Stable role identifier, used in roleNames and in currentUser.roles. Built-in roles use AppRole values.
  final String name;
  /// Localized by Accept-Language.
  final String displayName;
  final String nameAr;
  final String nameEn;
  /// True for the eight built-in roles. They cannot be deleted, and their name never changes.
  final bool isStatic;
  final int userCount;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;

  factory RoleDto.fromJson(Map<String, dynamic> json) => RoleDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        displayName: (json['displayName']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        isStatic: (json['isStatic']) as bool,
        userCount: ((json['userCount']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'displayName': displayName,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'isStatic': isStatic,
        'userCount': userCount,
        'concurrencyStamp': concurrencyStamp,
      };
}

class RolePagedResultDto {
  const RolePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<RoleDto> items;

  factory RolePagedResultDto.fromJson(Map<String, dynamic> json) => RolePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => RoleDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class RoleCreateDto {
  const RoleCreateDto({
    required this.name,
    required this.nameAr,
    required this.nameEn,
  });

  /// Unique identifier in camelCase. Must not equal an AppRole value.
  final String name;
  final String nameAr;
  final String nameEn;

  factory RoleCreateDto.fromJson(Map<String, dynamic> json) => RoleCreateDto(
        name: (json['name']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'name': name,
        'nameAr': nameAr,
        'nameEn': nameEn,
      };
}

class RoleUpdateDto {
  const RoleUpdateDto({
    required this.nameAr,
    required this.nameEn,
    required this.concurrencyStamp,
  });

  final String nameAr;
  final String nameEn;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;

  factory RoleUpdateDto.fromJson(Map<String, dynamic> json) => RoleUpdateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'concurrencyStamp': concurrencyStamp,
      };
}

class PermissionGrantInfoDto {
  const PermissionGrantInfoDto({
    required this.name,
    required this.displayName,
    this.parentName,
    required this.isGranted,
    required this.isDirectlyGranted,
    required this.grantedByRoleNames,
  });

  /// ABP permission name.
  final String name;
  /// Localized by Accept-Language.
  final String displayName;
  final String? parentName;
  /// Effective value for this role or user.
  final bool isGranted;
  /// Granted on this exact role or user, not inherited. Only this part can be changed here.
  final bool isDirectlyGranted;
  /// Roles that provide this permission. Always empty when reading a role.
  final List<String> grantedByRoleNames;

  factory PermissionGrantInfoDto.fromJson(Map<String, dynamic> json) => PermissionGrantInfoDto(
        name: (json['name']) as String,
        displayName: (json['displayName']) as String,
        parentName: json['parentName'] == null ? null : (json['parentName']) as String,
        isGranted: (json['isGranted']) as bool,
        isDirectlyGranted: (json['isDirectlyGranted']) as bool,
        grantedByRoleNames: ((json['grantedByRoleNames']) as List).map((e) => (e) as String).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'name': name,
        'displayName': displayName,
        if (parentName != null) 'parentName': parentName!,
        'isGranted': isGranted,
        'isDirectlyGranted': isDirectlyGranted,
        'grantedByRoleNames': grantedByRoleNames,
      };
}

class PermissionGroupDto {
  const PermissionGroupDto({
    required this.name,
    required this.displayName,
    required this.permissions,
  });

  final String name;
  /// Localized by Accept-Language.
  final String displayName;
  final List<PermissionGrantInfoDto> permissions;

  factory PermissionGroupDto.fromJson(Map<String, dynamic> json) => PermissionGroupDto(
        name: (json['name']) as String,
        displayName: (json['displayName']) as String,
        permissions: ((json['permissions']) as List).map((e) => PermissionGrantInfoDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'name': name,
        'displayName': displayName,
        'permissions': permissions.map((e) => e.toJson()).toList(),
      };
}

class PermissionListResultDto {
  const PermissionListResultDto({
    required this.groups,
  });

  /// Ordinary permissions only. Names starting with Hr.Complaint are absent.
  final List<PermissionGroupDto> groups;

  factory PermissionListResultDto.fromJson(Map<String, dynamic> json) => PermissionListResultDto(
        groups: ((json['groups']) as List).map((e) => PermissionGroupDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'groups': groups.map((e) => e.toJson()).toList(),
      };
}

class PermissionGrantChangeDto {
  const PermissionGrantChangeDto({
    required this.name,
    required this.isGranted,
  });

  final String name;
  final bool isGranted;

  factory PermissionGrantChangeDto.fromJson(Map<String, dynamic> json) => PermissionGrantChangeDto(
        name: (json['name']) as String,
        isGranted: (json['isGranted']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'name': name,
        'isGranted': isGranted,
      };
}

class PermissionGrantUpdateDto {
  const PermissionGrantUpdateDto({
    required this.permissions,
  });

  /// Only the permissions to change. Others stay as they are.
  final List<PermissionGrantChangeDto> permissions;

  factory PermissionGrantUpdateDto.fromJson(Map<String, dynamic> json) => PermissionGrantUpdateDto(
        permissions: ((json['permissions']) as List).map((e) => PermissionGrantChangeDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'permissions': permissions.map((e) => e.toJson()).toList(),
      };
}

/// Admin read of a department, for HR. Other roles use DepartmentRefDto through the lookup.
class DepartmentDto {
  const DepartmentDto({
    required this.id,
    required this.name,
    required this.nameAr,
    required this.nameEn,
    required this.employeeCount,
    required this.creationTime,
    this.lastModificationTime,
    required this.concurrencyStamp,
  });

  final String id;
  /// Localized by Accept-Language.
  final String name;
  final String nameAr;
  final String nameEn;
  /// Employees with status active or suspended.
  final int employeeCount;
  final DateTime creationTime;
  final DateTime? lastModificationTime;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;

  factory DepartmentDto.fromJson(Map<String, dynamic> json) => DepartmentDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        employeeCount: ((json['employeeCount']) as num).toInt(),
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'employeeCount': employeeCount,
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
        'concurrencyStamp': concurrencyStamp,
      };
}

class DepartmentPagedResultDto {
  const DepartmentPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<DepartmentDto> items;

  factory DepartmentPagedResultDto.fromJson(Map<String, dynamic> json) => DepartmentPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => DepartmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class DepartmentLookupPagedResultDto {
  const DepartmentLookupPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<DepartmentRefDto> items;

  factory DepartmentLookupPagedResultDto.fromJson(Map<String, dynamic> json) => DepartmentLookupPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => DepartmentRefDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class DepartmentCreateDto {
  const DepartmentCreateDto({
    required this.nameAr,
    required this.nameEn,
  });

  final String nameAr;
  final String nameEn;

  factory DepartmentCreateDto.fromJson(Map<String, dynamic> json) => DepartmentCreateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
      };
}

class DepartmentUpdateDto {
  const DepartmentUpdateDto({
    required this.nameAr,
    required this.nameEn,
    required this.concurrencyStamp,
  });

  final String nameAr;
  final String nameEn;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;

  factory DepartmentUpdateDto.fromJson(Map<String, dynamic> json) => DepartmentUpdateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'concurrencyStamp': concurrencyStamp,
      };
}

enum EmployeeStatus {
  active('active'),
  suspended('suspended'),
  ended('ended'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const EmployeeStatus(this.value);
  final String value;

  static EmployeeStatus fromJson(Object? json) {
    for (final v in EmployeeStatus.values) {
      if (v.value == json) return v;
    }
    return EmployeeStatus.$unknown;
  }

  String toJson() => value;
}

/// The shift in force today. Shift changes use PUT /api/app/employee/{id}/shift in the time and leave module.
class EmployeeShiftSummaryDto {
  const EmployeeShiftSummaryDto({
    required this.workShiftId,
    required this.name,
    required this.effectiveFrom,
  });

  final String workShiftId;
  /// Localized by Accept-Language.
  final String name;
  final DateTime effectiveFrom;

  factory EmployeeShiftSummaryDto.fromJson(Map<String, dynamic> json) => EmployeeShiftSummaryDto(
        workShiftId: (json['workShiftId']) as String,
        name: (json['name']) as String,
        effectiveFrom: hrDateFromJson((json['effectiveFrom']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'workShiftId': workShiftId,
        'name': name,
        'effectiveFrom': hrDateToJson(effectiveFrom),
      };
}

class EmployeeContractSummaryDto {
  const EmployeeContractSummaryDto({
    required this.contractId,
    required this.startDate,
    this.endDate,
  });

  /// Details come from the contracts module.
  final String contractId;
  final DateTime startDate;
  final DateTime? endDate;

  factory EmployeeContractSummaryDto.fromJson(Map<String, dynamic> json) => EmployeeContractSummaryDto(
        contractId: (json['contractId']) as String,
        startDate: hrDateFromJson((json['startDate']) as String),
        endDate: json['endDate'] == null ? null : hrDateFromJson((json['endDate']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'contractId': contractId,
        'startDate': hrDateToJson(startDate),
        if (endDate != null) 'endDate': hrDateToJson(endDate!),
      };
}

/// The employee reading their own file.
class EmployeeSelfDto {
  const EmployeeSelfDto({
    required this.id,
    required this.fullName,
    required this.fullNameAr,
    this.fullNameEn,
    this.email,
    this.phoneNumber,
    this.dateOfBirth,
    this.address,
    required this.department,
    required this.jobTitle,
    required this.hireDate,
    required this.status,
    this.manager,
    this.currentShift,
    this.currentContract,
    required this.roleNames,
    this.gender,
    this.employeeNumber,
  });

  final String id;
  /// Localized by Accept-Language.
  final String fullName;
  final String fullNameAr;
  final String? fullNameEn;
  final String? email;
  final String? phoneNumber;
  final DateTime? dateOfBirth;
  final String? address;
  final DepartmentRefDto department;
  final String jobTitle;
  /// Start of the personal leave year.
  final DateTime hireDate;
  final EmployeeStatus status;
  /// Null only for the executive director.
  final EmployeeRefDto? manager;
  /// Null when attendance does not apply, as for the executive director.
  final EmployeeShiftSummaryDto? currentShift;
  final EmployeeContractSummaryDto? currentContract;
  final List<String> roleNames;
  final AppGender? gender;
  /// الرقم الوظيفي.
  final String? employeeNumber;

  factory EmployeeSelfDto.fromJson(Map<String, dynamic> json) => EmployeeSelfDto(
        id: (json['id']) as String,
        fullName: (json['fullName']) as String,
        fullNameAr: (json['fullNameAr']) as String,
        fullNameEn: json['fullNameEn'] == null ? null : (json['fullNameEn']) as String,
        email: json['email'] == null ? null : (json['email']) as String,
        phoneNumber: json['phoneNumber'] == null ? null : (json['phoneNumber']) as String,
        dateOfBirth: json['dateOfBirth'] == null ? null : hrDateFromJson((json['dateOfBirth']) as String),
        address: json['address'] == null ? null : (json['address']) as String,
        department: DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        jobTitle: (json['jobTitle']) as String,
        hireDate: hrDateFromJson((json['hireDate']) as String),
        status: EmployeeStatus.fromJson(json['status']),
        manager: json['manager'] == null ? null : EmployeeRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        currentShift: json['currentShift'] == null ? null : EmployeeShiftSummaryDto.fromJson((json['currentShift']) as Map<String, dynamic>),
        currentContract: json['currentContract'] == null ? null : EmployeeContractSummaryDto.fromJson((json['currentContract']) as Map<String, dynamic>),
        roleNames: ((json['roleNames']) as List).map((e) => (e) as String).toList(),
        gender: json['gender'] == null ? null : AppGender.fromJson(json['gender']),
        employeeNumber: json['employeeNumber'] == null ? null : (json['employeeNumber']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'fullName': fullName,
        'fullNameAr': fullNameAr,
        if (fullNameEn != null) 'fullNameEn': fullNameEn!,
        if (email != null) 'email': email!,
        if (phoneNumber != null) 'phoneNumber': phoneNumber!,
        if (dateOfBirth != null) 'dateOfBirth': hrDateToJson(dateOfBirth!),
        if (address != null) 'address': address!,
        'department': department.toJson(),
        'jobTitle': jobTitle,
        'hireDate': hrDateToJson(hireDate),
        'status': status.toJson(),
        if (manager != null) 'manager': manager!.toJson(),
        if (currentShift != null) 'currentShift': currentShift!.toJson(),
        if (currentContract != null) 'currentContract': currentContract!.toJson(),
        'roleNames': roleNames,
        if (gender != null) 'gender': gender!.toJson(),
        if (employeeNumber != null) 'employeeNumber': employeeNumber!,
      };
}

/// Employee as seen by their direct manager. Financial data, complaints, contracts, birth date, address, roles and account data are absent by design. Do not add them.
class EmployeeTeamMemberDto {
  const EmployeeTeamMemberDto({
    required this.id,
    required this.fullName,
    this.email,
    this.phoneNumber,
    required this.department,
    required this.jobTitle,
    required this.hireDate,
    required this.status,
    this.currentShift,
  });

  final String id;
  /// Localized by Accept-Language.
  final String fullName;
  final String? email;
  final String? phoneNumber;
  final DepartmentRefDto department;
  final String jobTitle;
  final DateTime hireDate;
  final EmployeeStatus status;
  final EmployeeShiftSummaryDto? currentShift;

  factory EmployeeTeamMemberDto.fromJson(Map<String, dynamic> json) => EmployeeTeamMemberDto(
        id: (json['id']) as String,
        fullName: (json['fullName']) as String,
        email: json['email'] == null ? null : (json['email']) as String,
        phoneNumber: json['phoneNumber'] == null ? null : (json['phoneNumber']) as String,
        department: DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        jobTitle: (json['jobTitle']) as String,
        hireDate: hrDateFromJson((json['hireDate']) as String),
        status: EmployeeStatus.fromJson(json['status']),
        currentShift: json['currentShift'] == null ? null : EmployeeShiftSummaryDto.fromJson((json['currentShift']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'fullName': fullName,
        if (email != null) 'email': email!,
        if (phoneNumber != null) 'phoneNumber': phoneNumber!,
        'department': department.toJson(),
        'jobTitle': jobTitle,
        'hireDate': hrDateToJson(hireDate),
        'status': status.toJson(),
        if (currentShift != null) 'currentShift': currentShift!.toJson(),
      };
}

class EmployeeTeamMemberPagedResultDto {
  const EmployeeTeamMemberPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EmployeeTeamMemberDto> items;

  factory EmployeeTeamMemberPagedResultDto.fromJson(Map<String, dynamic> json) => EmployeeTeamMemberPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EmployeeTeamMemberDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EmployeeAccountSummaryDto {
  const EmployeeAccountSummaryDto({
    required this.userId,
    required this.userName,
    required this.isActive,
    required this.mustChangePassword,
    required this.mfaRequired,
    required this.mfaEnabled,
    required this.roleNames,
  });

  final String userId;
  final String userName;
  final bool isActive;
  final bool mustChangePassword;
  final bool mfaRequired;
  final bool mfaEnabled;
  final List<String> roleNames;

  factory EmployeeAccountSummaryDto.fromJson(Map<String, dynamic> json) => EmployeeAccountSummaryDto(
        userId: (json['userId']) as String,
        userName: (json['userName']) as String,
        isActive: (json['isActive']) as bool,
        mustChangePassword: (json['mustChangePassword']) as bool,
        mfaRequired: (json['mfaRequired']) as bool,
        mfaEnabled: (json['mfaEnabled']) as bool,
        roleNames: ((json['roleNames']) as List).map((e) => (e) as String).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'userId': userId,
        'userName': userName,
        'isActive': isActive,
        'mustChangePassword': mustChangePassword,
        'mfaRequired': mfaRequired,
        'mfaEnabled': mfaEnabled,
        'roleNames': roleNames,
      };
}

/// Full employee file for the HR manager. Salary data stays in the finance module and complaints stay in the complaints module, since both are isolated at service and database level.
class EmployeeHrDto {
  const EmployeeHrDto({
    required this.id,
    required this.fullName,
    required this.fullNameAr,
    this.fullNameEn,
    this.email,
    this.phoneNumber,
    this.dateOfBirth,
    this.address,
    required this.department,
    required this.jobTitle,
    required this.hireDate,
    required this.status,
    this.manager,
    this.currentShift,
    this.currentContract,
    this.account,
    required this.directReportCount,
    required this.creationTime,
    this.lastModificationTime,
    required this.concurrencyStamp,
    this.gender,
    this.employeeNumber,
  });

  final String id;
  /// Localized by Accept-Language.
  final String fullName;
  final String fullNameAr;
  final String? fullNameEn;
  final String? email;
  final String? phoneNumber;
  final DateTime? dateOfBirth;
  final String? address;
  final DepartmentRefDto department;
  final String jobTitle;
  final DateTime hireDate;
  final EmployeeStatus status;
  /// Null only for the executive director.
  final EmployeeRefDto? manager;
  final EmployeeShiftSummaryDto? currentShift;
  final EmployeeContractSummaryDto? currentContract;
  /// Null when the employee has no account yet.
  final EmployeeAccountSummaryDto? account;
  final int directReportCount;
  final DateTime creationTime;
  final DateTime? lastModificationTime;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;
  final AppGender? gender;
  /// الرقم الوظيفي.
  final String? employeeNumber;

  factory EmployeeHrDto.fromJson(Map<String, dynamic> json) => EmployeeHrDto(
        id: (json['id']) as String,
        fullName: (json['fullName']) as String,
        fullNameAr: (json['fullNameAr']) as String,
        fullNameEn: json['fullNameEn'] == null ? null : (json['fullNameEn']) as String,
        email: json['email'] == null ? null : (json['email']) as String,
        phoneNumber: json['phoneNumber'] == null ? null : (json['phoneNumber']) as String,
        dateOfBirth: json['dateOfBirth'] == null ? null : hrDateFromJson((json['dateOfBirth']) as String),
        address: json['address'] == null ? null : (json['address']) as String,
        department: DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        jobTitle: (json['jobTitle']) as String,
        hireDate: hrDateFromJson((json['hireDate']) as String),
        status: EmployeeStatus.fromJson(json['status']),
        manager: json['manager'] == null ? null : EmployeeRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        currentShift: json['currentShift'] == null ? null : EmployeeShiftSummaryDto.fromJson((json['currentShift']) as Map<String, dynamic>),
        currentContract: json['currentContract'] == null ? null : EmployeeContractSummaryDto.fromJson((json['currentContract']) as Map<String, dynamic>),
        account: json['account'] == null ? null : EmployeeAccountSummaryDto.fromJson((json['account']) as Map<String, dynamic>),
        directReportCount: ((json['directReportCount']) as num).toInt(),
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        gender: json['gender'] == null ? null : AppGender.fromJson(json['gender']),
        employeeNumber: json['employeeNumber'] == null ? null : (json['employeeNumber']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'fullName': fullName,
        'fullNameAr': fullNameAr,
        if (fullNameEn != null) 'fullNameEn': fullNameEn!,
        if (email != null) 'email': email!,
        if (phoneNumber != null) 'phoneNumber': phoneNumber!,
        if (dateOfBirth != null) 'dateOfBirth': hrDateToJson(dateOfBirth!),
        if (address != null) 'address': address!,
        'department': department.toJson(),
        'jobTitle': jobTitle,
        'hireDate': hrDateToJson(hireDate),
        'status': status.toJson(),
        if (manager != null) 'manager': manager!.toJson(),
        if (currentShift != null) 'currentShift': currentShift!.toJson(),
        if (currentContract != null) 'currentContract': currentContract!.toJson(),
        if (account != null) 'account': account!.toJson(),
        'directReportCount': directReportCount,
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
        'concurrencyStamp': concurrencyStamp,
        if (gender != null) 'gender': gender!.toJson(),
        if (employeeNumber != null) 'employeeNumber': employeeNumber!,
      };
}

class EmployeeListItemDto {
  const EmployeeListItemDto({
    required this.id,
    required this.fullName,
    required this.department,
    required this.jobTitle,
    required this.hireDate,
    required this.status,
    this.manager,
    required this.roleNames,
    required this.hasAccount,
  });

  final String id;
  /// Localized by Accept-Language.
  final String fullName;
  final DepartmentRefDto department;
  final String jobTitle;
  final DateTime hireDate;
  final EmployeeStatus status;
  final EmployeeRefDto? manager;
  final List<String> roleNames;
  final bool hasAccount;

  factory EmployeeListItemDto.fromJson(Map<String, dynamic> json) => EmployeeListItemDto(
        id: (json['id']) as String,
        fullName: (json['fullName']) as String,
        department: DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        jobTitle: (json['jobTitle']) as String,
        hireDate: hrDateFromJson((json['hireDate']) as String),
        status: EmployeeStatus.fromJson(json['status']),
        manager: json['manager'] == null ? null : EmployeeRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        roleNames: ((json['roleNames']) as List).map((e) => (e) as String).toList(),
        hasAccount: (json['hasAccount']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'fullName': fullName,
        'department': department.toJson(),
        'jobTitle': jobTitle,
        'hireDate': hrDateToJson(hireDate),
        'status': status.toJson(),
        if (manager != null) 'manager': manager!.toJson(),
        'roleNames': roleNames,
        'hasAccount': hasAccount,
      };
}

class EmployeeListItemPagedResultDto {
  const EmployeeListItemPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EmployeeListItemDto> items;

  factory EmployeeListItemPagedResultDto.fromJson(Map<String, dynamic> json) => EmployeeListItemPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EmployeeListItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EmployeeLookupPagedResultDto {
  const EmployeeLookupPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EmployeeRefDto> items;

  factory EmployeeLookupPagedResultDto.fromJson(Map<String, dynamic> json) => EmployeeLookupPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EmployeeRefDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EmployeeCreateDto {
  const EmployeeCreateDto({
    required this.fullNameAr,
    this.fullNameEn,
    required this.email,
    this.phoneNumber,
    this.dateOfBirth,
    this.address,
    required this.departmentId,
    required this.jobTitle,
    required this.hireDate,
    this.workShiftId,
    this.managerEmployeeId,
    required this.userName,
    required this.roleNames,
    this.gender,
    this.employeeNumber,
  });

  final String fullNameAr;
  final String? fullNameEn;
  /// Required for every employee. An account email that is left empty takes this value.
  final String email;
  final String? phoneNumber;
  final DateTime? dateOfBirth;
  final String? address;
  final String departmentId;
  final String jobTitle;
  final DateTime hireDate;
  /// Required for everyone except the executive director. Takes effect from hireDate.
  final String? workShiftId;
  /// Required, and must follow x-global-rules.manager-link. Must be null for the executive director.
  final String? managerEmployeeId;
  /// Account user name.
  final String userName;
  /// Role names, one or more. The server always adds employee.
  final List<String> roleNames;
  final AppGender? gender;
  /// الرقم الوظيفي.
  final String? employeeNumber;

  factory EmployeeCreateDto.fromJson(Map<String, dynamic> json) => EmployeeCreateDto(
        fullNameAr: (json['fullNameAr']) as String,
        fullNameEn: json['fullNameEn'] == null ? null : (json['fullNameEn']) as String,
        email: (json['email']) as String,
        phoneNumber: json['phoneNumber'] == null ? null : (json['phoneNumber']) as String,
        dateOfBirth: json['dateOfBirth'] == null ? null : hrDateFromJson((json['dateOfBirth']) as String),
        address: json['address'] == null ? null : (json['address']) as String,
        departmentId: (json['departmentId']) as String,
        jobTitle: (json['jobTitle']) as String,
        hireDate: hrDateFromJson((json['hireDate']) as String),
        workShiftId: json['workShiftId'] == null ? null : (json['workShiftId']) as String,
        managerEmployeeId: json['managerEmployeeId'] == null ? null : (json['managerEmployeeId']) as String,
        userName: (json['userName']) as String,
        roleNames: ((json['roleNames']) as List).map((e) => (e) as String).toList(),
        gender: json['gender'] == null ? null : AppGender.fromJson(json['gender']),
        employeeNumber: json['employeeNumber'] == null ? null : (json['employeeNumber']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'fullNameAr': fullNameAr,
        if (fullNameEn != null) 'fullNameEn': fullNameEn!,
        'email': email,
        if (phoneNumber != null) 'phoneNumber': phoneNumber!,
        if (dateOfBirth != null) 'dateOfBirth': hrDateToJson(dateOfBirth!),
        if (address != null) 'address': address!,
        'departmentId': departmentId,
        'jobTitle': jobTitle,
        'hireDate': hrDateToJson(hireDate),
        if (workShiftId != null) 'workShiftId': workShiftId!,
        if (managerEmployeeId != null) 'managerEmployeeId': managerEmployeeId!,
        'userName': userName,
        'roleNames': roleNames,
        if (gender != null) 'gender': gender!.toJson(),
        if (employeeNumber != null) 'employeeNumber': employeeNumber!,
      };
}

class EmployeeCreatedDto {
  const EmployeeCreatedDto({
    required this.employee,
    required this.credentials,
  });

  final EmployeeHrDto employee;
  final TemporaryCredentialsDto credentials;

  factory EmployeeCreatedDto.fromJson(Map<String, dynamic> json) => EmployeeCreatedDto(
        employee: EmployeeHrDto.fromJson((json['employee']) as Map<String, dynamic>),
        credentials: TemporaryCredentialsDto.fromJson((json['credentials']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'credentials': credentials.toJson(),
      };
}

class EmployeeHireDateCorrectionDto {
  const EmployeeHireDateCorrectionDto({
    required this.hireDate,
    required this.reason,
    required this.concurrencyStamp,
  });

  final DateTime hireDate;
  final String reason;
  final String concurrencyStamp;

  factory EmployeeHireDateCorrectionDto.fromJson(Map<String, dynamic> json) => EmployeeHireDateCorrectionDto(
        hireDate: hrDateFromJson((json['hireDate']) as String),
        reason: (json['reason']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'hireDate': hrDateToJson(hireDate),
        'reason': reason,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// Manager, shift and roles are not here. Each has its own route, because each has its own history and side effects.
class EmployeeUpdateDto {
  const EmployeeUpdateDto({
    required this.fullNameAr,
    this.fullNameEn,
    required this.email,
    this.phoneNumber,
    this.dateOfBirth,
    this.address,
    required this.departmentId,
    required this.jobTitle,
    required this.hireDate,
    required this.status,
    required this.concurrencyStamp,
    this.gender,
    this.employeeNumber,
  });

  final String fullNameAr;
  final String? fullNameEn;
  /// Required for every employee. An account email that is left empty takes this value.
  final String email;
  final String? phoneNumber;
  final DateTime? dateOfBirth;
  final String? address;
  final String departmentId;
  final String jobTitle;
  final DateTime hireDate;
  final EmployeeStatus status;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;
  final AppGender? gender;
  /// الرقم الوظيفي.
  final String? employeeNumber;

  factory EmployeeUpdateDto.fromJson(Map<String, dynamic> json) => EmployeeUpdateDto(
        fullNameAr: (json['fullNameAr']) as String,
        fullNameEn: json['fullNameEn'] == null ? null : (json['fullNameEn']) as String,
        email: (json['email']) as String,
        phoneNumber: json['phoneNumber'] == null ? null : (json['phoneNumber']) as String,
        dateOfBirth: json['dateOfBirth'] == null ? null : hrDateFromJson((json['dateOfBirth']) as String),
        address: json['address'] == null ? null : (json['address']) as String,
        departmentId: (json['departmentId']) as String,
        jobTitle: (json['jobTitle']) as String,
        hireDate: hrDateFromJson((json['hireDate']) as String),
        status: EmployeeStatus.fromJson(json['status']),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        gender: json['gender'] == null ? null : AppGender.fromJson(json['gender']),
        employeeNumber: json['employeeNumber'] == null ? null : (json['employeeNumber']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'fullNameAr': fullNameAr,
        if (fullNameEn != null) 'fullNameEn': fullNameEn!,
        'email': email,
        if (phoneNumber != null) 'phoneNumber': phoneNumber!,
        if (dateOfBirth != null) 'dateOfBirth': hrDateToJson(dateOfBirth!),
        if (address != null) 'address': address!,
        'departmentId': departmentId,
        'jobTitle': jobTitle,
        'hireDate': hrDateToJson(hireDate),
        'status': status.toJson(),
        'concurrencyStamp': concurrencyStamp,
        if (gender != null) 'gender': gender!.toJson(),
        if (employeeNumber != null) 'employeeNumber': employeeNumber!,
      };
}

class ManagerChangeInputDto {
  const ManagerChangeInputDto({
    required this.managerEmployeeId,
    this.reason,
    required this.concurrencyStamp,
  });

  /// The new manager, as an employee id.
  final String managerEmployeeId;
  final String? reason;
  /// Optimistic concurrency token. Send the latest value; a mismatch returns 409.
  final String concurrencyStamp;

  factory ManagerChangeInputDto.fromJson(Map<String, dynamic> json) => ManagerChangeInputDto(
        managerEmployeeId: (json['managerEmployeeId']) as String,
        reason: json['reason'] == null ? null : (json['reason']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'managerEmployeeId': managerEmployeeId,
        if (reason != null) 'reason': reason!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class ManagerChangeResultDto {
  const ManagerChangeResultDto({
    required this.employee,
    this.previousManager,
    required this.newManager,
    required this.effectiveFrom,
    required this.movedPendingItemCount,
    required this.concurrencyStamp,
  });

  final EmployeeRefDto employee;
  final EmployeeRefDto? previousManager;
  final EmployeeRefDto newManager;
  final DateTime effectiveFrom;
  /// Pending items moved to the new manager: requests, memos and incidents. Complaints are never counted.
  final int movedPendingItemCount;
  final String concurrencyStamp;

  factory ManagerChangeResultDto.fromJson(Map<String, dynamic> json) => ManagerChangeResultDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        previousManager: json['previousManager'] == null ? null : EmployeeRefDto.fromJson((json['previousManager']) as Map<String, dynamic>),
        newManager: EmployeeRefDto.fromJson((json['newManager']) as Map<String, dynamic>),
        effectiveFrom: hrDateFromJson((json['effectiveFrom']) as String),
        movedPendingItemCount: ((json['movedPendingItemCount']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        if (previousManager != null) 'previousManager': previousManager!.toJson(),
        'newManager': newManager.toJson(),
        'effectiveFrom': hrDateToJson(effectiveFrom),
        'movedPendingItemCount': movedPendingItemCount,
        'concurrencyStamp': concurrencyStamp,
      };
}

class ManagerAssignEmployeesInputDto {
  const ManagerAssignEmployeesInputDto({
    required this.employeeIds,
    this.reason,
  });

  final List<String> employeeIds;
  final String? reason;

  factory ManagerAssignEmployeesInputDto.fromJson(Map<String, dynamic> json) => ManagerAssignEmployeesInputDto(
        employeeIds: ((json['employeeIds']) as List).map((e) => (e) as String).toList(),
        reason: json['reason'] == null ? null : (json['reason']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employeeIds': employeeIds,
        if (reason != null) 'reason': reason!,
      };
}

class ManagerAssignEmployeesResultDto {
  const ManagerAssignEmployeesResultDto({
    required this.manager,
    required this.assignedCount,
    required this.alreadyAssignedCount,
    required this.movedPendingItemCount,
    required this.items,
  });

  final EmployeeRefDto manager;
  final int assignedCount;
  /// Employees already linked to this manager. No history row is written for them.
  final int alreadyAssignedCount;
  final int movedPendingItemCount;
  /// One item for each employee actually moved.
  final List<ManagerChangeResultDto> items;

  factory ManagerAssignEmployeesResultDto.fromJson(Map<String, dynamic> json) => ManagerAssignEmployeesResultDto(
        manager: EmployeeRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        assignedCount: ((json['assignedCount']) as num).toInt(),
        alreadyAssignedCount: ((json['alreadyAssignedCount']) as num).toInt(),
        movedPendingItemCount: ((json['movedPendingItemCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ManagerChangeResultDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'manager': manager.toJson(),
        'assignedCount': assignedCount,
        'alreadyAssignedCount': alreadyAssignedCount,
        'movedPendingItemCount': movedPendingItemCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EmployeeManagerHistoryDto {
  const EmployeeManagerHistoryDto({
    required this.id,
    required this.employeeId,
    required this.manager,
    required this.fromDate,
    this.toDate,
    this.changedBy,
    this.reason,
  });

  final String id;
  final String employeeId;
  final EmployeeRefDto manager;
  final DateTime fromDate;
  /// Null on the current link.
  final DateTime? toDate;
  final UserRefDto? changedBy;
  final String? reason;

  factory EmployeeManagerHistoryDto.fromJson(Map<String, dynamic> json) => EmployeeManagerHistoryDto(
        id: (json['id']) as String,
        employeeId: (json['employeeId']) as String,
        manager: EmployeeRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        fromDate: hrDateFromJson((json['fromDate']) as String),
        toDate: json['toDate'] == null ? null : hrDateFromJson((json['toDate']) as String),
        changedBy: json['changedBy'] == null ? null : UserRefDto.fromJson((json['changedBy']) as Map<String, dynamic>),
        reason: json['reason'] == null ? null : (json['reason']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employeeId': employeeId,
        'manager': manager.toJson(),
        'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate!),
        if (changedBy != null) 'changedBy': changedBy!.toJson(),
        if (reason != null) 'reason': reason!,
      };
}

class EmployeeManagerHistoryPagedResultDto {
  const EmployeeManagerHistoryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EmployeeManagerHistoryDto> items;

  factory EmployeeManagerHistoryPagedResultDto.fromJson(Map<String, dynamic> json) => EmployeeManagerHistoryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EmployeeManagerHistoryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

enum EmployeeDocumentType {
  identity('identity'),
  passport('passport'),
  contract('contract'),
  certificate('certificate'),
  medical('medical'),
  other('other'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const EmployeeDocumentType(this.value);
  final String value;

  static EmployeeDocumentType fromJson(Object? json) {
    for (final v in EmployeeDocumentType.values) {
      if (v.value == json) return v;
    }
    return EmployeeDocumentType.$unknown;
  }

  String toJson() => value;
}

/// normal: HR, the employee and the direct manager. sensitive: HR and the employee only.
enum DocumentSensitivity {
  normal('normal'),
  sensitive('sensitive'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const DocumentSensitivity(this.value);
  final String value;

  static DocumentSensitivity fromJson(Object? json) {
    for (final v in DocumentSensitivity.values) {
      if (v.value == json) return v;
    }
    return DocumentSensitivity.$unknown;
  }

  String toJson() => value;
}

/// Virus scan result. Only clean documents can be downloaded.
enum DocumentScanStatus {
  pending('pending'),
  clean('clean'),
  infected('infected'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const DocumentScanStatus(this.value);
  final String value;

  static DocumentScanStatus fromJson(Object? json) {
    for (final v in DocumentScanStatus.values) {
      if (v.value == json) return v;
    }
    return DocumentScanStatus.$unknown;
  }

  String toJson() => value;
}

/// Document metadata. The storage blob name is never exposed.
class EmployeeDocumentDto {
  const EmployeeDocumentDto({
    required this.id,
    required this.employeeId,
    required this.type,
    required this.title,
    required this.fileName,
    required this.contentType,
    required this.sizeBytes,
    required this.sensitivity,
    required this.scanStatus,
    this.uploadedBy,
    required this.creationTime,
  });

  final String id;
  final String employeeId;
  final EmployeeDocumentType type;
  final String title;
  final String fileName;
  final String contentType;
  final int sizeBytes;
  final DocumentSensitivity sensitivity;
  final DocumentScanStatus scanStatus;
  final UserRefDto? uploadedBy;
  final DateTime creationTime;

  factory EmployeeDocumentDto.fromJson(Map<String, dynamic> json) => EmployeeDocumentDto(
        id: (json['id']) as String,
        employeeId: (json['employeeId']) as String,
        type: EmployeeDocumentType.fromJson(json['type']),
        title: (json['title']) as String,
        fileName: (json['fileName']) as String,
        contentType: (json['contentType']) as String,
        sizeBytes: ((json['sizeBytes']) as num).toInt(),
        sensitivity: DocumentSensitivity.fromJson(json['sensitivity']),
        scanStatus: DocumentScanStatus.fromJson(json['scanStatus']),
        uploadedBy: json['uploadedBy'] == null ? null : UserRefDto.fromJson((json['uploadedBy']) as Map<String, dynamic>),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employeeId': employeeId,
        'type': type.toJson(),
        'title': title,
        'fileName': fileName,
        'contentType': contentType,
        'sizeBytes': sizeBytes,
        'sensitivity': sensitivity.toJson(),
        'scanStatus': scanStatus.toJson(),
        if (uploadedBy != null) 'uploadedBy': uploadedBy!.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class EmployeeDocumentPagedResultDto {
  const EmployeeDocumentPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EmployeeDocumentDto> items;

  factory EmployeeDocumentPagedResultDto.fromJson(Map<String, dynamic> json) => EmployeeDocumentPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EmployeeDocumentDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EmployeeDocumentUploadDto {
  const EmployeeDocumentUploadDto({
    required this.file,
    required this.type,
    required this.title,
    required this.sensitivity,
  });

  final MultipartFile file;
  final EmployeeDocumentType type;
  final String title;
  final DocumentSensitivity sensitivity;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'type': type.toJson(),
        'title': title,
        'sensitivity': sensitivity.toJson(),
      };

  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,
  /// one part per file for arrays (x-server-requirements.multipart).
  FormData toFormData() {
    final form = FormData();
    form.files.add(MapEntry('file', file));
    form.fields.add(MapEntry('type', hrFormValue(type.toJson())));
    form.fields.add(MapEntry('title', hrFormValue(title)));
    form.fields.add(MapEntry('sensitivity', hrFormValue(sensitivity.toJson())));
    return form;
  }
}

/// Which approval work moves to the delegate. Complaint handling, finance and evaluations can never be delegated.
enum DelegationScope {
  allApprovals('allApprovals'),
  requests('requests'),
  memosAndIncidents('memosAndIncidents'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const DelegationScope(this.value);
  final String value;

  static DelegationScope fromJson(Object? json) {
    for (final v in DelegationScope.values) {
      if (v.value == json) return v;
    }
    return DelegationScope.$unknown;
  }

  String toJson() => value;
}

/// Computed by the server from the dates and cancellation.
enum DelegationStatus {
  scheduled('scheduled'),
  active('active'),
  expired('expired'),
  cancelled('cancelled'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const DelegationStatus(this.value);
  final String value;

  static DelegationStatus fromJson(Object? json) {
    for (final v in DelegationStatus.values) {
      if (v.value == json) return v;
    }
    return DelegationStatus.$unknown;
  }

  String toJson() => value;
}

enum DelegationDirection {
  given('given'),
  received('received'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const DelegationDirection(this.value);
  final String value;

  static DelegationDirection fromJson(Object? json) {
    for (final v in DelegationDirection.values) {
      if (v.value == json) return v;
    }
    return DelegationDirection.$unknown;
  }

  String toJson() => value;
}

class DelegationDto {
  const DelegationDto({
    required this.id,
    required this.fromUser,
    required this.toUser,
    required this.scope,
    required this.startsAt,
    required this.endsAt,
    required this.reason,
    required this.status,
    this.cancelledAt,
    required this.creationTime,
  });

  final String id;
  final UserRefDto fromUser;
  final UserRefDto toUser;
  final DelegationScope scope;
  final DateTime startsAt;
  final DateTime endsAt;
  final String reason;
  final DelegationStatus status;
  final DateTime? cancelledAt;
  final DateTime creationTime;

  factory DelegationDto.fromJson(Map<String, dynamic> json) => DelegationDto(
        id: (json['id']) as String,
        fromUser: UserRefDto.fromJson((json['fromUser']) as Map<String, dynamic>),
        toUser: UserRefDto.fromJson((json['toUser']) as Map<String, dynamic>),
        scope: DelegationScope.fromJson(json['scope']),
        startsAt: DateTime.parse((json['startsAt']) as String),
        endsAt: DateTime.parse((json['endsAt']) as String),
        reason: (json['reason']) as String,
        status: DelegationStatus.fromJson(json['status']),
        cancelledAt: json['cancelledAt'] == null ? null : DateTime.parse((json['cancelledAt']) as String),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'fromUser': fromUser.toJson(),
        'toUser': toUser.toJson(),
        'scope': scope.toJson(),
        'startsAt': hrDateTimeToJson(startsAt),
        'endsAt': hrDateTimeToJson(endsAt),
        'reason': reason,
        'status': status.toJson(),
        if (cancelledAt != null) 'cancelledAt': hrDateTimeToJson(cancelledAt!),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class DelegationPagedResultDto {
  const DelegationPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<DelegationDto> items;

  factory DelegationPagedResultDto.fromJson(Map<String, dynamic> json) => DelegationPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => DelegationDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class DelegationCreateDto {
  const DelegationCreateDto({
    this.fromUserId,
    required this.toUserId,
    required this.scope,
    required this.startsAt,
    required this.endsAt,
    required this.reason,
  });

  /// HR only: the absent approver being delegated for. Empty means the caller. Recorded in the task log.
  final String? fromUserId;
  /// The delegate. Choose from GET /api/app/delegation/candidate.
  final String toUserId;
  final DelegationScope scope;
  /// Now or later.
  final DateTime startsAt;
  /// After startsAt.
  final DateTime endsAt;
  final String reason;

  factory DelegationCreateDto.fromJson(Map<String, dynamic> json) => DelegationCreateDto(
        fromUserId: json['fromUserId'] == null ? null : (json['fromUserId']) as String,
        toUserId: (json['toUserId']) as String,
        scope: DelegationScope.fromJson(json['scope']),
        startsAt: DateTime.parse((json['startsAt']) as String),
        endsAt: DateTime.parse((json['endsAt']) as String),
        reason: (json['reason']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (fromUserId != null) 'fromUserId': fromUserId!,
        'toUserId': toUserId,
        'scope': scope.toJson(),
        'startsAt': hrDateTimeToJson(startsAt),
        'endsAt': hrDateTimeToJson(endsAt),
        'reason': reason,
      };
}

class DelegationCandidatePagedResultDto {
  const DelegationCandidatePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<UserRefDto> items;

  factory DelegationCandidatePagedResultDto.fromJson(Map<String, dynamic> json) => DelegationCandidatePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => UserRefDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// fixed = ثابت، rotating = متناوب، overnight = ليلي يمتد بعد منتصف الليل.
enum WorkShiftType {
  fixed('fixed'),
  rotating('rotating'),
  overnight('overnight'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WorkShiftType(this.value);
  final String value;

  static WorkShiftType fromJson(Object? json) {
    for (final v in WorkShiftType.values) {
      if (v.value == json) return v;
    }
    return WorkShiftType.$unknown;
  }

  String toJson() => value;
}

/// none = لا يُحتسب عمل إضافي. afterShiftEnd = كل ما بعد نهاية الوردية المجدولة. afterDailyHours = ما يتجاوز overtimeAfterHours من ساعات العمل الفعلية في اليوم.
enum WorkShiftOvertimePolicy {
  none('none'),
  afterShiftEnd('afterShiftEnd'),
  afterDailyHours('afterDailyHours'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WorkShiftOvertimePolicy(this.value);
  final String value;

  static WorkShiftOvertimePolicy fromJson(Object? json) {
    for (final v in WorkShiftOvertimePolicy.values) {
      if (v.value == json) return v;
    }
    return WorkShiftOvertimePolicy.$unknown;
  }

  String toJson() => value;
}

enum WorkShiftDayOfWeek {
  sunday('sunday'),
  monday('monday'),
  tuesday('tuesday'),
  wednesday('wednesday'),
  thursday('thursday'),
  friday('friday'),
  saturday('saturday'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WorkShiftDayOfWeek(this.value);
  final String value;

  static WorkShiftDayOfWeek fromJson(Object? json) {
    for (final v in WorkShiftDayOfWeek.values) {
      if (v.value == json) return v;
    }
    return WorkShiftDayOfWeek.$unknown;
  }

  String toJson() => value;
}

/// يوم عمل واحد في القالب. الأيام غير المدرجة أيام راحة أسبوعية للموظف ولا تُحسب من الإجازة.
class WorkShiftDayDto {
  const WorkShiftDayDto({
    this.dayOfWeek,
    this.cycleDayNumber,
    required this.startTime,
    required this.endTime,
    required this.crossesMidnight,
  });

  /// إلزامي للقالب fixed وovernight، ويجب أن يكون null للقالب rotating. يوم الوردية هو اليوم الذي تبدأ فيه.
  final WorkShiftDayOfWeek? dayOfWeek;
  /// رقم اليوم داخل دورة التناوب من 1 إلى rotationCycleDays. إلزامي للقالب rotating، ويجب أن يكون null لغيره. اليوم غير المدرج في الدورة يوم راحة.
  final int? cycleDayNumber;
  /// وقت البداية بتوقيت الشركة.
  final String startTime;
  /// وقت النهاية بتوقيت الشركة.
  final String endTime;
  /// true إذا كان endTime في اليوم التالي. يجب أن يساوي (endTime <= startTime)، وإلا يُرفض بـHr:Shift:CrossesMidnightMismatch.
  final bool crossesMidnight;

  factory WorkShiftDayDto.fromJson(Map<String, dynamic> json) => WorkShiftDayDto(
        dayOfWeek: json['dayOfWeek'] == null ? null : WorkShiftDayOfWeek.fromJson(json['dayOfWeek']),
        cycleDayNumber: json['cycleDayNumber'] == null ? null : ((json['cycleDayNumber']) as num).toInt(),
        startTime: (json['startTime']) as String,
        endTime: (json['endTime']) as String,
        crossesMidnight: (json['crossesMidnight']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (dayOfWeek != null) 'dayOfWeek': dayOfWeek!.toJson(),
        if (cycleDayNumber != null) 'cycleDayNumber': cycleDayNumber!,
        'startTime': startTime,
        'endTime': endTime,
        'crossesMidnight': crossesMidnight,
      };
}

class WorkShiftCreateDto {
  const WorkShiftCreateDto({
    required this.nameAr,
    required this.nameEn,
    required this.type,
    required this.graceMinutes,
    required this.overtimePolicy,
    this.overtimeAfterHours,
    this.rotationCycleDays,
    required this.days,
  });

  final String nameAr;
  final String nameEn;
  final WorkShiftType type;
  /// دقائق السماح بالتأخير. لا يُحسب التأخر إلا بعد تجاوزها، وعندها يُحسب من بداية الوردية.
  final int graceMinutes;
  final WorkShiftOvertimePolicy overtimePolicy;
  /// إلزامي عندما overtimePolicy = afterDailyHours، ويجب أن يكون null لغيرها.
  final double? overtimeAfterHours;
  /// طول دورة التناوب بالأيام. إلزامي للقالب rotating، ويجب أن يكون null لغيره. اليوم 1 من الدورة هو تاريخ بداية ربط الموظف بالقالب.
  final int? rotationCycleDays;
  /// أيام القالب. لا يتكرر اليوم نفسه.
  final List<WorkShiftDayDto> days;

  factory WorkShiftCreateDto.fromJson(Map<String, dynamic> json) => WorkShiftCreateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        type: WorkShiftType.fromJson(json['type']),
        graceMinutes: ((json['graceMinutes']) as num).toInt(),
        overtimePolicy: WorkShiftOvertimePolicy.fromJson(json['overtimePolicy']),
        overtimeAfterHours: json['overtimeAfterHours'] == null ? null : ((json['overtimeAfterHours']) as num).toDouble(),
        rotationCycleDays: json['rotationCycleDays'] == null ? null : ((json['rotationCycleDays']) as num).toInt(),
        days: ((json['days']) as List).map((e) => WorkShiftDayDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'type': type.toJson(),
        'graceMinutes': graceMinutes,
        'overtimePolicy': overtimePolicy.toJson(),
        if (overtimeAfterHours != null) 'overtimeAfterHours': overtimeAfterHours!,
        if (rotationCycleDays != null) 'rotationCycleDays': rotationCycleDays!,
        'days': days.map((e) => e.toJson()).toList(),
      };
}

class WorkShiftUpdateDto {
  const WorkShiftUpdateDto({
    required this.nameAr,
    required this.nameEn,
    required this.type,
    required this.graceMinutes,
    required this.overtimePolicy,
    this.overtimeAfterHours,
    this.rotationCycleDays,
    required this.days,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String nameAr;
  final String nameEn;
  final WorkShiftType type;
  /// دقائق السماح بالتأخير. لا يُحسب التأخر إلا بعد تجاوزها، وعندها يُحسب من بداية الوردية.
  final int graceMinutes;
  final WorkShiftOvertimePolicy overtimePolicy;
  /// إلزامي عندما overtimePolicy = afterDailyHours، ويجب أن يكون null لغيرها.
  final double? overtimeAfterHours;
  /// طول دورة التناوب بالأيام. إلزامي للقالب rotating، ويجب أن يكون null لغيره. اليوم 1 من الدورة هو تاريخ بداية ربط الموظف بالقالب.
  final int? rotationCycleDays;
  /// أيام القالب. لا يتكرر اليوم نفسه.
  final List<WorkShiftDayDto> days;
  /// القالب غير المفعل لا يُربط به موظف جديد، ويبقى ساريًا لمن رُبط به سابقًا.
  final bool isActive;
  final String concurrencyStamp;

  factory WorkShiftUpdateDto.fromJson(Map<String, dynamic> json) => WorkShiftUpdateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        type: WorkShiftType.fromJson(json['type']),
        graceMinutes: ((json['graceMinutes']) as num).toInt(),
        overtimePolicy: WorkShiftOvertimePolicy.fromJson(json['overtimePolicy']),
        overtimeAfterHours: json['overtimeAfterHours'] == null ? null : ((json['overtimeAfterHours']) as num).toDouble(),
        rotationCycleDays: json['rotationCycleDays'] == null ? null : ((json['rotationCycleDays']) as num).toInt(),
        days: ((json['days']) as List).map((e) => WorkShiftDayDto.fromJson((e) as Map<String, dynamic>)).toList(),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'type': type.toJson(),
        'graceMinutes': graceMinutes,
        'overtimePolicy': overtimePolicy.toJson(),
        if (overtimeAfterHours != null) 'overtimeAfterHours': overtimeAfterHours!,
        if (rotationCycleDays != null) 'rotationCycleDays': rotationCycleDays!,
        'days': days.map((e) => e.toJson()).toList(),
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

class WorkShiftDto {
  const WorkShiftDto({
    required this.id,
    required this.name,
    required this.nameAr,
    required this.nameEn,
    required this.type,
    required this.graceMinutes,
    required this.overtimePolicy,
    this.overtimeAfterHours,
    this.rotationCycleDays,
    required this.days,
    required this.isActive,
    required this.assignedEmployeeCount,
    required this.creationTime,
    this.lastModificationTime,
    required this.concurrencyStamp,
  });

  final String id;
  /// Localized by Accept-Language.
  final String name;
  final String nameAr;
  final String nameEn;
  final WorkShiftType type;
  final int graceMinutes;
  final WorkShiftOvertimePolicy overtimePolicy;
  final double? overtimeAfterHours;
  final int? rotationCycleDays;
  final List<WorkShiftDayDto> days;
  final bool isActive;
  /// عدد الموظفين المرتبطين بالقالب حاليًا.
  final int assignedEmployeeCount;
  final DateTime creationTime;
  final DateTime? lastModificationTime;
  final String concurrencyStamp;

  factory WorkShiftDto.fromJson(Map<String, dynamic> json) => WorkShiftDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        type: WorkShiftType.fromJson(json['type']),
        graceMinutes: ((json['graceMinutes']) as num).toInt(),
        overtimePolicy: WorkShiftOvertimePolicy.fromJson(json['overtimePolicy']),
        overtimeAfterHours: json['overtimeAfterHours'] == null ? null : ((json['overtimeAfterHours']) as num).toDouble(),
        rotationCycleDays: json['rotationCycleDays'] == null ? null : ((json['rotationCycleDays']) as num).toInt(),
        days: ((json['days']) as List).map((e) => WorkShiftDayDto.fromJson((e) as Map<String, dynamic>)).toList(),
        isActive: (json['isActive']) as bool,
        assignedEmployeeCount: ((json['assignedEmployeeCount']) as num).toInt(),
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'type': type.toJson(),
        'graceMinutes': graceMinutes,
        'overtimePolicy': overtimePolicy.toJson(),
        if (overtimeAfterHours != null) 'overtimeAfterHours': overtimeAfterHours!,
        if (rotationCycleDays != null) 'rotationCycleDays': rotationCycleDays!,
        'days': days.map((e) => e.toJson()).toList(),
        'isActive': isActive,
        'assignedEmployeeCount': assignedEmployeeCount,
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
        'concurrencyStamp': concurrencyStamp,
      };
}

class WorkShiftRefDto {
  const WorkShiftRefDto({
    required this.id,
    required this.name,
    required this.type,
  });

  final String id;
  /// Localized by Accept-Language.
  final String name;
  final WorkShiftType type;

  factory WorkShiftRefDto.fromJson(Map<String, dynamic> json) => WorkShiftRefDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        type: WorkShiftType.fromJson(json['type']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'type': type.toJson(),
      };
}

class WorkShiftPagedResultDto {
  const WorkShiftPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<WorkShiftDto> items;

  factory WorkShiftPagedResultDto.fromJson(Map<String, dynamic> json) => WorkShiftPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => WorkShiftDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EmployeeShiftAssignDto {
  const EmployeeShiftAssignDto({
    required this.shiftId,
    required this.startDate,
  });

  final String shiftId;
  /// تاريخ بداية الدوام الجديد. لا يسبق تاريخ مباشرة الموظف، ولا بداية آخر ربط له.
  final DateTime startDate;

  factory EmployeeShiftAssignDto.fromJson(Map<String, dynamic> json) => EmployeeShiftAssignDto(
        shiftId: (json['shiftId']) as String,
        startDate: hrDateFromJson((json['startDate']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'shiftId': shiftId,
        'startDate': hrDateToJson(startDate),
      };
}

class EmployeeShiftDto {
  const EmployeeShiftDto({
    required this.id,
    required this.employee,
    required this.shift,
    required this.startDate,
    this.endDate,
    required this.creationTime,
    this.createdBy,
  });

  final String id;
  final EmployeeRefDto employee;
  final WorkShiftRefDto shift;
  final DateTime startDate;
  /// آخر يوم في هذا الربط. null يعني أنه الربط الساري.
  final DateTime? endDate;
  final DateTime creationTime;
  final UserRefDto? createdBy;

  factory EmployeeShiftDto.fromJson(Map<String, dynamic> json) => EmployeeShiftDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        shift: WorkShiftRefDto.fromJson((json['shift']) as Map<String, dynamic>),
        startDate: hrDateFromJson((json['startDate']) as String),
        endDate: json['endDate'] == null ? null : hrDateFromJson((json['endDate']) as String),
        creationTime: DateTime.parse((json['creationTime']) as String),
        createdBy: json['createdBy'] == null ? null : UserRefDto.fromJson((json['createdBy']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'shift': shift.toJson(),
        'startDate': hrDateToJson(startDate),
        if (endDate != null) 'endDate': hrDateToJson(endDate!),
        'creationTime': hrDateTimeToJson(creationTime),
        if (createdBy != null) 'createdBy': createdBy!.toJson(),
      };
}

class EmployeeShiftPagedResultDto {
  const EmployeeShiftPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<EmployeeShiftDto> items;

  factory EmployeeShiftPagedResultDto.fromJson(Map<String, dynamic> json) => EmployeeShiftPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EmployeeShiftDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class HolidayWriteDto {
  const HolidayWriteDto({
    required this.date,
    required this.nameAr,
    required this.nameEn,
  });

  /// العطلة ليوم واحد. العطلة الممتدة تُدخل بسجل لكل يوم.
  final DateTime date;
  final String nameAr;
  final String nameEn;

  factory HolidayWriteDto.fromJson(Map<String, dynamic> json) => HolidayWriteDto(
        date: hrDateFromJson((json['date']) as String),
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'date': hrDateToJson(date),
        'nameAr': nameAr,
        'nameEn': nameEn,
      };
}

class HolidayUpdateDto {
  const HolidayUpdateDto({
    required this.date,
    required this.nameAr,
    required this.nameEn,
    required this.concurrencyStamp,
  });

  final DateTime date;
  final String nameAr;
  final String nameEn;
  final String concurrencyStamp;

  factory HolidayUpdateDto.fromJson(Map<String, dynamic> json) => HolidayUpdateDto(
        date: hrDateFromJson((json['date']) as String),
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'date': hrDateToJson(date),
        'nameAr': nameAr,
        'nameEn': nameEn,
        'concurrencyStamp': concurrencyStamp,
      };
}

class HolidayDto {
  const HolidayDto({
    required this.id,
    required this.date,
    required this.name,
    required this.nameAr,
    required this.nameEn,
    required this.concurrencyStamp,
  });

  final String id;
  final DateTime date;
  /// Localized by Accept-Language.
  final String name;
  final String nameAr;
  final String nameEn;
  final String concurrencyStamp;

  factory HolidayDto.fromJson(Map<String, dynamic> json) => HolidayDto(
        id: (json['id']) as String,
        date: hrDateFromJson((json['date']) as String),
        name: (json['name']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'date': hrDateToJson(date),
        'name': name,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'concurrencyStamp': concurrencyStamp,
      };
}

class HolidayPagedResultDto {
  const HolidayPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<HolidayDto> items;

  factory HolidayPagedResultDto.fromJson(Map<String, dynamic> json) => HolidayPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => HolidayDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class WeeklyOffSettingDto {
  const WeeklyOffSettingDto({
    required this.days,
    required this.concurrencyStamp,
  });

  /// أيام العطلة الأسبوعية في تقويم الشركة.
  final List<WorkShiftDayOfWeek> days;
  final String concurrencyStamp;

  factory WeeklyOffSettingDto.fromJson(Map<String, dynamic> json) => WeeklyOffSettingDto(
        days: ((json['days']) as List).map((e) => WorkShiftDayOfWeek.fromJson(e)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'days': days.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
      };
}

class WeeklyOffSettingUpdateDto {
  const WeeklyOffSettingUpdateDto({
    required this.days,
    required this.concurrencyStamp,
  });

  final List<WorkShiftDayOfWeek> days;
  final String concurrencyStamp;

  factory WeeklyOffSettingUpdateDto.fromJson(Map<String, dynamic> json) => WeeklyOffSettingUpdateDto(
        days: ((json['days']) as List).map((e) => WorkShiftDayOfWeek.fromJson(e)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'days': days.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
      };
}

/// device = البصمة، gps = GPS عند إضافته، both = الاثنان.
enum AttendanceCaptureMethod {
  device('device'),
  gps('gps'),
  both('both'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AttendanceCaptureMethod(this.value);
  final String value;

  static AttendanceCaptureMethod fromJson(Object? json) {
    for (final v in AttendanceCaptureMethod.values) {
      if (v.value == json) return v;
    }
    return AttendanceCaptureMethod.$unknown;
  }

  String toJson() => value;
}

class AttendanceSettingsDto {
  const AttendanceSettingsDto({
    required this.method,
    required this.gpsAvailable,
    required this.timeZone,
    required this.concurrencyStamp,
  });

  final AttendanceCaptureMethod method;
  /// false حتى إطلاق ميزة GPS. عندها لا تظهر خيارات gps وboth ولا زر تسجيل الحضور بالموقع.
  final bool gpsAvailable;
  /// معرف IANA لتوقيت الشركة. تُفسَّر به كل حقول الوقت HH:mm وتحديد اليوم.
  final String timeZone;
  final String concurrencyStamp;

  factory AttendanceSettingsDto.fromJson(Map<String, dynamic> json) => AttendanceSettingsDto(
        method: AttendanceCaptureMethod.fromJson(json['method']),
        gpsAvailable: (json['gpsAvailable']) as bool,
        timeZone: (json['timeZone']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'method': method.toJson(),
        'gpsAvailable': gpsAvailable,
        'timeZone': timeZone,
        'concurrencyStamp': concurrencyStamp,
      };
}

class AttendanceSettingsUpdateDto {
  const AttendanceSettingsUpdateDto({
    required this.method,
    required this.timeZone,
    required this.concurrencyStamp,
  });

  final AttendanceCaptureMethod method;
  /// معرف IANA صالح.
  final String timeZone;
  final String concurrencyStamp;

  factory AttendanceSettingsUpdateDto.fromJson(Map<String, dynamic> json) => AttendanceSettingsUpdateDto(
        method: AttendanceCaptureMethod.fromJson(json['method']),
        timeZone: (json['timeZone']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'method': method.toJson(),
        'timeZone': timeZone,
        'concurrencyStamp': concurrencyStamp,
      };
}

enum AttendancePunchSource {
  device('device'),
  file('file'),
  gps('gps'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AttendancePunchSource(this.value);
  final String value;

  static AttendancePunchSource fromJson(Object? json) {
    for (final v in AttendancePunchSource.values) {
      if (v.value == json) return v;
    }
    return AttendancePunchSource.$unknown;
  }

  String toJson() => value;
}

/// present = حاضر. late = متأخر بعد السماح. absent = غائب في يوم دوام. incomplete = بصمة واحدة فقط. onLeave = إجازة معتمدة تغطي اليوم. holiday = عطلة رسمية. weeklyOff = يوم راحة في وردية الموظف. noShift = لا يوجد ربط دوام في هذا التاريخ.
enum AttendanceDayStatus {
  present('present'),
  late('late'),
  absent('absent'),
  incomplete('incomplete'),
  onLeave('onLeave'),
  holiday('holiday'),
  weeklyOff('weeklyOff'),
  noShift('noShift'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AttendanceDayStatus(this.value);
  final String value;

  static AttendanceDayStatus fromJson(Object? json) {
    for (final v in AttendanceDayStatus.values) {
      if (v.value == json) return v;
    }
    return AttendanceDayStatus.$unknown;
  }

  String toJson() => value;
}

/// نوع المشكلة في طلب التصحيح: دخول أو خروج.
enum AttendancePunchType {
  checkIn('checkIn'),
  checkOut('checkOut'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AttendancePunchType(this.value);
  final String value;

  static AttendancePunchType fromJson(Object? json) {
    for (final v in AttendancePunchType.values) {
      if (v.value == json) return v;
    }
    return AttendancePunchType.$unknown;
  }

  String toJson() => value;
}

class AttendanceDayDto {
  const AttendanceDayDto({
    required this.id,
    required this.employee,
    required this.date,
    this.shift,
    this.scheduledStart,
    this.scheduledEnd,
    this.checkIn,
    this.checkOut,
    required this.status,
    required this.lateMinutes,
    required this.workedMinutes,
    required this.overtimeMinutes,
    this.hourlyLeaveMinutes,
    this.leaveRequestId,
    required this.isCorrected,
  });

  final String id;
  final EmployeeRefDto employee;
  /// يوم الوردية. الوردية الليلية تُنسب كلها إلى اليوم الذي بدأت فيه.
  final DateTime date;
  final WorkShiftRefDto? shift;
  /// بداية الدوام المجدولة. null إذا لم يكن يوم دوام.
  final DateTime? scheduledStart;
  /// نهاية الدوام المجدولة، وقد تقع في اليوم التالي للوردية الليلية.
  final DateTime? scheduledEnd;
  /// الدخول المعتمد الحالي بعد أي تصحيح.
  final DateTime? checkIn;
  /// الخروج المعتمد الحالي بعد أي تصحيح.
  final DateTime? checkOut;
  final AttendanceDayStatus status;
  /// دقائق التأخر من بداية الوردية إذا تجاوزت السماح، وإلا 0. لا تشمل وقتًا مغطى بإجازة زمنية معتمدة.
  final int lateMinutes;
  /// ساعات الدوام الفعلية بالدقائق.
  final int workedMinutes;
  /// حسب سياسة القالب.
  final int overtimeMinutes;
  /// دقائق الإجازة الزمنية المعتمدة في هذا اليوم.
  final int? hourlyLeaveMinutes;
  /// طلب الإجازة المعتمد الذي يغطي اليوم أو جزءًا منه. يساوي معرف الطلب العام.
  final String? leaveRequestId;
  /// true إذا طُبق على اليوم تصحيح معتمد.
  final bool isCorrected;

  factory AttendanceDayDto.fromJson(Map<String, dynamic> json) => AttendanceDayDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        date: hrDateFromJson((json['date']) as String),
        shift: json['shift'] == null ? null : WorkShiftRefDto.fromJson((json['shift']) as Map<String, dynamic>),
        scheduledStart: json['scheduledStart'] == null ? null : DateTime.parse((json['scheduledStart']) as String),
        scheduledEnd: json['scheduledEnd'] == null ? null : DateTime.parse((json['scheduledEnd']) as String),
        checkIn: json['checkIn'] == null ? null : DateTime.parse((json['checkIn']) as String),
        checkOut: json['checkOut'] == null ? null : DateTime.parse((json['checkOut']) as String),
        status: AttendanceDayStatus.fromJson(json['status']),
        lateMinutes: ((json['lateMinutes']) as num).toInt(),
        workedMinutes: ((json['workedMinutes']) as num).toInt(),
        overtimeMinutes: ((json['overtimeMinutes']) as num).toInt(),
        hourlyLeaveMinutes: json['hourlyLeaveMinutes'] == null ? null : ((json['hourlyLeaveMinutes']) as num).toInt(),
        leaveRequestId: json['leaveRequestId'] == null ? null : (json['leaveRequestId']) as String,
        isCorrected: (json['isCorrected']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'date': hrDateToJson(date),
        if (shift != null) 'shift': shift!.toJson(),
        if (scheduledStart != null) 'scheduledStart': hrDateTimeToJson(scheduledStart!),
        if (scheduledEnd != null) 'scheduledEnd': hrDateTimeToJson(scheduledEnd!),
        if (checkIn != null) 'checkIn': hrDateTimeToJson(checkIn!),
        if (checkOut != null) 'checkOut': hrDateTimeToJson(checkOut!),
        'status': status.toJson(),
        'lateMinutes': lateMinutes,
        'workedMinutes': workedMinutes,
        'overtimeMinutes': overtimeMinutes,
        if (hourlyLeaveMinutes != null) 'hourlyLeaveMinutes': hourlyLeaveMinutes!,
        if (leaveRequestId != null) 'leaveRequestId': leaveRequestId!,
        'isCorrected': isCorrected,
      };
}

/// مجاميع كل السجلات المطابقة للمرشحات، وليست الصفحة الحالية فقط.
class AttendancePeriodSummaryDto {
  const AttendancePeriodSummaryDto({
    required this.presentDays,
    required this.lateDays,
    required this.totalLateMinutes,
    required this.absentDays,
    required this.incompleteDays,
    required this.leaveDays,
    required this.workedMinutes,
    required this.overtimeMinutes,
  });

  /// يشمل الأيام المتأخرة.
  final int presentDays;
  final int lateDays;
  final int totalLateMinutes;
  final int absentDays;
  final int incompleteDays;
  final double leaveDays;
  final int workedMinutes;
  final int overtimeMinutes;

  factory AttendancePeriodSummaryDto.fromJson(Map<String, dynamic> json) => AttendancePeriodSummaryDto(
        presentDays: ((json['presentDays']) as num).toInt(),
        lateDays: ((json['lateDays']) as num).toInt(),
        totalLateMinutes: ((json['totalLateMinutes']) as num).toInt(),
        absentDays: ((json['absentDays']) as num).toInt(),
        incompleteDays: ((json['incompleteDays']) as num).toInt(),
        leaveDays: ((json['leaveDays']) as num).toDouble(),
        workedMinutes: ((json['workedMinutes']) as num).toInt(),
        overtimeMinutes: ((json['overtimeMinutes']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'presentDays': presentDays,
        'lateDays': lateDays,
        'totalLateMinutes': totalLateMinutes,
        'absentDays': absentDays,
        'incompleteDays': incompleteDays,
        'leaveDays': leaveDays,
        'workedMinutes': workedMinutes,
        'overtimeMinutes': overtimeMinutes,
      };
}

class AttendanceDayPagedResultDto {
  const AttendanceDayPagedResultDto({
    required this.totalCount,
    required this.items,
    required this.summary,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<AttendanceDayDto> items;
  final AttendancePeriodSummaryDto summary;

  factory AttendanceDayPagedResultDto.fromJson(Map<String, dynamic> json) => AttendanceDayPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AttendanceDayDto.fromJson((e) as Map<String, dynamic>)).toList(),
        summary: AttendancePeriodSummaryDto.fromJson((json['summary']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
        'summary': summary.toJson(),
      };
}

/// عملية خام ثابتة. لا تحتوي بيانات حيوية، ولا إحداثيات الموقع.
class AttendanceRawPunchDto {
  const AttendanceRawPunchDto({
    required this.id,
    required this.punchTime,
    required this.source,
    this.deviceName,
    this.importId,
  });

  final String id;
  /// الوقت الخام كما ورد. لا يُعدَّل أبدًا.
  final DateTime punchTime;
  final AttendancePunchSource source;
  /// اسم الجهاز إذا كان المصدر جهازًا أو ملفًا مرتبطًا بجهاز.
  final String? deviceName;
  /// عملية الاستيراد إذا كان المصدر ملفًا.
  final String? importId;

  factory AttendanceRawPunchDto.fromJson(Map<String, dynamic> json) => AttendanceRawPunchDto(
        id: (json['id']) as String,
        punchTime: DateTime.parse((json['punchTime']) as String),
        source: AttendancePunchSource.fromJson(json['source']),
        deviceName: json['deviceName'] == null ? null : (json['deviceName']) as String,
        importId: json['importId'] == null ? null : (json['importId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'punchTime': hrDateTimeToJson(punchTime),
        'source': source.toJson(),
        if (deviceName != null) 'deviceName': deviceName!,
        if (importId != null) 'importId': importId!,
      };
}

class AttendanceCorrectionHistoryEntryDto {
  const AttendanceCorrectionHistoryEntryDto({
    required this.id,
    required this.requestId,
    required this.punchType,
    this.oldValue,
    required this.newValue,
    required this.reason,
    required this.appliedAt,
    required this.requestedBy,
    this.approvedBy,
    required this.affectsClosedMonth,
    this.settlementPeriod,
  });

  final String id;
  /// الطلب العام الذي اعتُمد التصحيح من خلاله.
  final String requestId;
  final AttendancePunchType punchType;
  /// القيمة قبل هذا التعديل. null إذا لم تكن هناك بصمة.
  final DateTime? oldValue;
  /// القيمة بعد هذا التعديل.
  final DateTime newValue;
  /// السبب الذي كتبه مقدم الطلب.
  final String reason;
  /// وقت الاعتماد النهائي والتطبيق.
  final DateTime appliedAt;
  final UserRefDto requestedBy;
  /// معتمد الخطوة الأخيرة.
  final UserRefDto? approvedBy;
  /// true إذا كان الشهر مغلقًا عند التطبيق، فيظهر الأثر المالي تسوية في الشهر التالي.
  final bool affectsClosedMonth;
  /// الشهر الذي تظهر فيه التسوية عند الإغلاق بصيغة yyyy-MM. لا يتضمن أي مبلغ.
  final String? settlementPeriod;

  factory AttendanceCorrectionHistoryEntryDto.fromJson(Map<String, dynamic> json) => AttendanceCorrectionHistoryEntryDto(
        id: (json['id']) as String,
        requestId: (json['requestId']) as String,
        punchType: AttendancePunchType.fromJson(json['punchType']),
        oldValue: json['oldValue'] == null ? null : DateTime.parse((json['oldValue']) as String),
        newValue: DateTime.parse((json['newValue']) as String),
        reason: (json['reason']) as String,
        appliedAt: DateTime.parse((json['appliedAt']) as String),
        requestedBy: UserRefDto.fromJson((json['requestedBy']) as Map<String, dynamic>),
        approvedBy: json['approvedBy'] == null ? null : UserRefDto.fromJson((json['approvedBy']) as Map<String, dynamic>),
        affectsClosedMonth: (json['affectsClosedMonth']) as bool,
        settlementPeriod: json['settlementPeriod'] == null ? null : (json['settlementPeriod']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'requestId': requestId,
        'punchType': punchType.toJson(),
        if (oldValue != null) 'oldValue': hrDateTimeToJson(oldValue!),
        'newValue': hrDateTimeToJson(newValue),
        'reason': reason,
        'appliedAt': hrDateTimeToJson(appliedAt),
        'requestedBy': requestedBy.toJson(),
        if (approvedBy != null) 'approvedBy': approvedBy!.toJson(),
        'affectsClosedMonth': affectsClosedMonth,
        if (settlementPeriod != null) 'settlementPeriod': settlementPeriod!,
      };
}

class AttendanceDayDetailDto {
  const AttendanceDayDetailDto({
    required this.day,
    this.originalCheckIn,
    this.originalCheckOut,
    required this.rawPunches,
    required this.corrections,
  });

  final AttendanceDayDto day;
  /// الدخول المحسوب من العمليات الخام قبل أي تصحيح.
  final DateTime? originalCheckIn;
  /// الخروج المحسوب من العمليات الخام قبل أي تصحيح.
  final DateTime? originalCheckOut;
  /// كل العمليات الخام لهذا اليوم، مرتبة زمنيًا.
  final List<AttendanceRawPunchDto> rawPunches;
  /// سجل التعديل كاملًا، من الأقدم إلى الأحدث.
  final List<AttendanceCorrectionHistoryEntryDto> corrections;

  factory AttendanceDayDetailDto.fromJson(Map<String, dynamic> json) => AttendanceDayDetailDto(
        day: AttendanceDayDto.fromJson((json['day']) as Map<String, dynamic>),
        originalCheckIn: json['originalCheckIn'] == null ? null : DateTime.parse((json['originalCheckIn']) as String),
        originalCheckOut: json['originalCheckOut'] == null ? null : DateTime.parse((json['originalCheckOut']) as String),
        rawPunches: ((json['rawPunches']) as List).map((e) => AttendanceRawPunchDto.fromJson((e) as Map<String, dynamic>)).toList(),
        corrections: ((json['corrections']) as List).map((e) => AttendanceCorrectionHistoryEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'day': day.toJson(),
        if (originalCheckIn != null) 'originalCheckIn': hrDateTimeToJson(originalCheckIn!),
        if (originalCheckOut != null) 'originalCheckOut': hrDateTimeToJson(originalCheckOut!),
        'rawPunches': rawPunches.map((e) => e.toJson()).toList(),
        'corrections': corrections.map((e) => e.toJson()).toList(),
      };
}

/// وقت حضور فقط. يُمنع إرسال قوالب البصمات أو أي بيانات حيوية، وأي خاصية إضافية تُرفض.
class AttendancePunchInputDto {
  const AttendancePunchInputDto({
    required this.deviceUserId,
    required this.punchTime,
  });

  /// رقم المستخدم داخل الجهاز.
  final String deviceUserId;
  /// وقت العملية بصيغة UTC. على الوسيط تحويل توقيت الجهاز قبل الإرسال.
  final DateTime punchTime;

  factory AttendancePunchInputDto.fromJson(Map<String, dynamic> json) => AttendancePunchInputDto(
        deviceUserId: (json['deviceUserId']) as String,
        punchTime: DateTime.parse((json['punchTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'deviceUserId': deviceUserId,
        'punchTime': hrDateTimeToJson(punchTime),
      };
}

class AttendancePunchBatchDto {
  const AttendancePunchBatchDto({
    required this.punches,
  });

  /// يجوز أن تتضمن عمليات أُرسلت سابقًا، فهي لا تتكرر.
  final List<AttendancePunchInputDto> punches;

  factory AttendancePunchBatchDto.fromJson(Map<String, dynamic> json) => AttendancePunchBatchDto(
        punches: ((json['punches']) as List).map((e) => AttendancePunchInputDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'punches': punches.map((e) => e.toJson()).toList(),
      };
}

/// accepted = حُفظت وربطت بموظف. duplicate = موجودة سابقًا ولم يتغير شيء. unmappedDeviceUser = حُفظت خامًا دون موظف، وتُربط تلقائيًا عند إضافة الربط. invalid = لم تُحفظ، مثل وقت في المستقبل بأكثر من 5 دقائق.
enum AttendancePunchResultStatus {
  accepted('accepted'),
  duplicate('duplicate'),
  unmappedDeviceUser('unmappedDeviceUser'),
  invalid('invalid'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AttendancePunchResultStatus(this.value);
  final String value;

  static AttendancePunchResultStatus fromJson(Object? json) {
    for (final v in AttendancePunchResultStatus.values) {
      if (v.value == json) return v;
    }
    return AttendancePunchResultStatus.$unknown;
  }

  String toJson() => value;
}

class AttendancePunchIssueDto {
  const AttendancePunchIssueDto({
    required this.index,
    required this.deviceUserId,
    this.punchTime,
    required this.status,
  });

  /// موقع العملية في مصفوفة الطلب، ابتداءً من 0.
  final int index;
  final String deviceUserId;
  final DateTime? punchTime;
  final AttendancePunchResultStatus status;

  factory AttendancePunchIssueDto.fromJson(Map<String, dynamic> json) => AttendancePunchIssueDto(
        index: ((json['index']) as num).toInt(),
        deviceUserId: (json['deviceUserId']) as String,
        punchTime: json['punchTime'] == null ? null : DateTime.parse((json['punchTime']) as String),
        status: AttendancePunchResultStatus.fromJson(json['status']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'index': index,
        'deviceUserId': deviceUserId,
        if (punchTime != null) 'punchTime': hrDateTimeToJson(punchTime!),
        'status': status.toJson(),
      };
}

class AttendancePunchBatchResultDto {
  const AttendancePunchBatchResultDto({
    required this.receivedCount,
    required this.acceptedCount,
    required this.duplicateCount,
    required this.unmappedCount,
    required this.invalidCount,
    required this.issues,
  });

  final int receivedCount;
  final int acceptedCount;
  final int duplicateCount;
  final int unmappedCount;
  final int invalidCount;
  /// العمليات غير المقبولة أو غير المربوطة فقط، ولا تشمل التكرار. على الوسيط عدم إعادة إرسال invalid.
  final List<AttendancePunchIssueDto> issues;

  factory AttendancePunchBatchResultDto.fromJson(Map<String, dynamic> json) => AttendancePunchBatchResultDto(
        receivedCount: ((json['receivedCount']) as num).toInt(),
        acceptedCount: ((json['acceptedCount']) as num).toInt(),
        duplicateCount: ((json['duplicateCount']) as num).toInt(),
        unmappedCount: ((json['unmappedCount']) as num).toInt(),
        invalidCount: ((json['invalidCount']) as num).toInt(),
        issues: ((json['issues']) as List).map((e) => AttendancePunchIssueDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'receivedCount': receivedCount,
        'acceptedCount': acceptedCount,
        'duplicateCount': duplicateCount,
        'unmappedCount': unmappedCount,
        'invalidCount': invalidCount,
        'issues': issues.map((e) => e.toJson()).toList(),
      };
}

enum AttendanceImportStatus {
  queued('queued'),
  processing('processing'),
  completed('completed'),
  completedWithErrors('completedWithErrors'),
  failed('failed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AttendanceImportStatus(this.value);
  final String value;

  static AttendanceImportStatus fromJson(Object? json) {
    for (final v in AttendanceImportStatus.values) {
      if (v.value == json) return v;
    }
    return AttendanceImportStatus.$unknown;
  }

  String toJson() => value;
}

class AttendanceImportUploadDto {
  const AttendanceImportUploadDto({
    required this.file,
    required this.deviceId,
  });

  /// ملف .xlsx أو .csv بعمودين إلزاميين في الصف الأول: deviceUserId وpunchDateTime بصيغة yyyy-MM-dd HH:mm بتوقيت الشركة. الحد الأقصى 20 ميجابايت.
  final MultipartFile file;
  /// الجهاز الذي جاءت منه البيانات، وتُستخدم خرائطه لربط الموظفين.
  final String deviceId;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'deviceId': deviceId,
      };

  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,
  /// one part per file for arrays (x-server-requirements.multipart).
  FormData toFormData() {
    final form = FormData();
    form.files.add(MapEntry('file', file));
    form.fields.add(MapEntry('deviceId', hrFormValue(deviceId)));
    return form;
  }
}

enum AttendanceImportErrorCode {
  missingColumn('missingColumn'),
  invalidDateTime('invalidDateTime'),
  emptyDeviceUserId('emptyDeviceUserId'),
  futurePunch('futurePunch'),
  invalidDeviceUserId('invalidDeviceUserId'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AttendanceImportErrorCode(this.value);
  final String value;

  static AttendanceImportErrorCode fromJson(Object? json) {
    for (final v in AttendanceImportErrorCode.values) {
      if (v.value == json) return v;
    }
    return AttendanceImportErrorCode.$unknown;
  }

  String toJson() => value;
}

class AttendanceImportErrorDto {
  const AttendanceImportErrorDto({
    required this.rowNumber,
    this.columnName,
    required this.code,
    required this.message,
  });

  /// رقم الصف في الملف.
  final int rowNumber;
  final String? columnName;
  final AttendanceImportErrorCode code;
  /// Localized by Accept-Language.
  final String message;

  factory AttendanceImportErrorDto.fromJson(Map<String, dynamic> json) => AttendanceImportErrorDto(
        rowNumber: ((json['rowNumber']) as num).toInt(),
        columnName: json['columnName'] == null ? null : (json['columnName']) as String,
        code: AttendanceImportErrorCode.fromJson(json['code']),
        message: (json['message']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'rowNumber': rowNumber,
        if (columnName != null) 'columnName': columnName!,
        'code': code.toJson(),
        'message': message,
      };
}

class AttendanceImportDto {
  const AttendanceImportDto({
    required this.id,
    required this.fileName,
    required this.deviceId,
    required this.status,
    required this.totalRows,
    required this.acceptedCount,
    required this.duplicateCount,
    required this.unmappedCount,
    required this.errorCount,
    required this.errors,
    required this.uploadedBy,
    required this.creationTime,
    this.completedAt,
  });

  final String id;
  final String fileName;
  final String deviceId;
  final AttendanceImportStatus status;
  final int totalRows;
  final int acceptedCount;
  /// صفوف موجودة سابقًا، ولا تُعد خطأ.
  final int duplicateCount;
  /// صفوف حُفظت خامًا دون موظف.
  final int unmappedCount;
  final int errorCount;
  final List<AttendanceImportErrorDto> errors;
  final UserRefDto uploadedBy;
  final DateTime creationTime;
  final DateTime? completedAt;

  factory AttendanceImportDto.fromJson(Map<String, dynamic> json) => AttendanceImportDto(
        id: (json['id']) as String,
        fileName: (json['fileName']) as String,
        deviceId: (json['deviceId']) as String,
        status: AttendanceImportStatus.fromJson(json['status']),
        totalRows: ((json['totalRows']) as num).toInt(),
        acceptedCount: ((json['acceptedCount']) as num).toInt(),
        duplicateCount: ((json['duplicateCount']) as num).toInt(),
        unmappedCount: ((json['unmappedCount']) as num).toInt(),
        errorCount: ((json['errorCount']) as num).toInt(),
        errors: ((json['errors']) as List).map((e) => AttendanceImportErrorDto.fromJson((e) as Map<String, dynamic>)).toList(),
        uploadedBy: UserRefDto.fromJson((json['uploadedBy']) as Map<String, dynamic>),
        creationTime: DateTime.parse((json['creationTime']) as String),
        completedAt: json['completedAt'] == null ? null : DateTime.parse((json['completedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'fileName': fileName,
        'deviceId': deviceId,
        'status': status.toJson(),
        'totalRows': totalRows,
        'acceptedCount': acceptedCount,
        'duplicateCount': duplicateCount,
        'unmappedCount': unmappedCount,
        'errorCount': errorCount,
        'errors': errors.map((e) => e.toJson()).toList(),
        'uploadedBy': uploadedBy.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
        if (completedAt != null) 'completedAt': hrDateTimeToJson(completedAt!),
      };
}

class AttendanceImportPagedResultDto {
  const AttendanceImportPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<AttendanceImportDto> items;

  factory AttendanceImportPagedResultDto.fromJson(Map<String, dynamic> json) => AttendanceImportPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AttendanceImportDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class AttendanceDeviceCreateDto {
  const AttendanceDeviceCreateDto({
    required this.serialNumber,
    required this.name,
  });

  final String serialNumber;
  final String name;

  factory AttendanceDeviceCreateDto.fromJson(Map<String, dynamic> json) => AttendanceDeviceCreateDto(
        serialNumber: (json['serialNumber']) as String,
        name: (json['name']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'serialNumber': serialNumber,
        'name': name,
      };
}

class AttendanceDeviceUpdateDto {
  const AttendanceDeviceUpdateDto({
    required this.name,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String name;
  /// الجهاز غير المفعل تُرفض عملياته بـ401.
  final bool isActive;
  final String concurrencyStamp;

  factory AttendanceDeviceUpdateDto.fromJson(Map<String, dynamic> json) => AttendanceDeviceUpdateDto(
        name: (json['name']) as String,
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'name': name,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// لا يحتوي مفتاح الجهاز أبدًا.
class AttendanceDeviceDto {
  const AttendanceDeviceDto({
    required this.id,
    required this.serialNumber,
    required this.name,
    required this.isActive,
    this.lastSeenAt,
    required this.mappedUserCount,
    required this.unmappedUserCount,
    required this.creationTime,
    required this.concurrencyStamp,
  });

  final String id;
  final String serialNumber;
  final String name;
  final bool isActive;
  /// آخر اتصال ناجح بمفتاح الجهاز.
  final DateTime? lastSeenAt;
  final int mappedUserCount;
  /// مستخدمو الجهاز الذين وردت لهم عمليات دون ربط بموظف.
  final int unmappedUserCount;
  final DateTime creationTime;
  final String concurrencyStamp;

  factory AttendanceDeviceDto.fromJson(Map<String, dynamic> json) => AttendanceDeviceDto(
        id: (json['id']) as String,
        serialNumber: (json['serialNumber']) as String,
        name: (json['name']) as String,
        isActive: (json['isActive']) as bool,
        lastSeenAt: json['lastSeenAt'] == null ? null : DateTime.parse((json['lastSeenAt']) as String),
        mappedUserCount: ((json['mappedUserCount']) as num).toInt(),
        unmappedUserCount: ((json['unmappedUserCount']) as num).toInt(),
        creationTime: DateTime.parse((json['creationTime']) as String),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'serialNumber': serialNumber,
        'name': name,
        'isActive': isActive,
        if (lastSeenAt != null) 'lastSeenAt': hrDateTimeToJson(lastSeenAt!),
        'mappedUserCount': mappedUserCount,
        'unmappedUserCount': unmappedUserCount,
        'creationTime': hrDateTimeToJson(creationTime),
        'concurrencyStamp': concurrencyStamp,
      };
}

class AttendanceDeviceKeyDto {
  const AttendanceDeviceKeyDto({
    required this.device,
    required this.deviceKey,
  });

  final AttendanceDeviceDto device;
  /// يُعرض مرة واحدة فقط في هذا الرد، ويُخزن مجزأً (hash) في الخادم. يُرسل في الترويسة X-Device-Key.
  final String deviceKey;

  factory AttendanceDeviceKeyDto.fromJson(Map<String, dynamic> json) => AttendanceDeviceKeyDto(
        device: AttendanceDeviceDto.fromJson((json['device']) as Map<String, dynamic>),
        deviceKey: (json['deviceKey']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'device': device.toJson(),
        'deviceKey': deviceKey,
      };
}

class AttendanceDevicePagedResultDto {
  const AttendanceDevicePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<AttendanceDeviceDto> items;

  factory AttendanceDevicePagedResultDto.fromJson(Map<String, dynamic> json) => AttendanceDevicePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AttendanceDeviceDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class DeviceUserMapCreateDto {
  const DeviceUserMapCreateDto({
    required this.deviceUserId,
    required this.employeeId,
  });

  final String deviceUserId;
  final String employeeId;

  factory DeviceUserMapCreateDto.fromJson(Map<String, dynamic> json) => DeviceUserMapCreateDto(
        deviceUserId: (json['deviceUserId']) as String,
        employeeId: (json['employeeId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'deviceUserId': deviceUserId,
        'employeeId': employeeId,
      };
}

class DeviceUserMapDto {
  const DeviceUserMapDto({
    required this.id,
    required this.deviceId,
    required this.deviceUserId,
    required this.employee,
    required this.creationTime,
  });

  final String id;
  final String deviceId;
  final String deviceUserId;
  final EmployeeRefDto employee;
  final DateTime creationTime;

  factory DeviceUserMapDto.fromJson(Map<String, dynamic> json) => DeviceUserMapDto(
        id: (json['id']) as String,
        deviceId: (json['deviceId']) as String,
        deviceUserId: (json['deviceUserId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'deviceId': deviceId,
        'deviceUserId': deviceUserId,
        'employee': employee.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class DeviceUserMapPagedResultDto {
  const DeviceUserMapPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<DeviceUserMapDto> items;

  factory DeviceUserMapPagedResultDto.fromJson(Map<String, dynamic> json) => DeviceUserMapPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => DeviceUserMapDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class UnmappedDeviceUserDto {
  const UnmappedDeviceUserDto({
    required this.deviceUserId,
    required this.punchCount,
    required this.firstPunchTime,
    required this.lastPunchTime,
  });

  final String deviceUserId;
  final int punchCount;
  final DateTime firstPunchTime;
  final DateTime lastPunchTime;

  factory UnmappedDeviceUserDto.fromJson(Map<String, dynamic> json) => UnmappedDeviceUserDto(
        deviceUserId: (json['deviceUserId']) as String,
        punchCount: ((json['punchCount']) as num).toInt(),
        firstPunchTime: DateTime.parse((json['firstPunchTime']) as String),
        lastPunchTime: DateTime.parse((json['lastPunchTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'deviceUserId': deviceUserId,
        'punchCount': punchCount,
        'firstPunchTime': hrDateTimeToJson(firstPunchTime),
        'lastPunchTime': hrDateTimeToJson(lastPunchTime),
      };
}

class UnmappedDeviceUserPagedResultDto {
  const UnmappedDeviceUserPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<UnmappedDeviceUserDto> items;

  factory UnmappedDeviceUserPagedResultDto.fromJson(Map<String, dynamic> json) => UnmappedDeviceUserPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => UnmappedDeviceUserDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class AttendanceCorrectionCreateDto {
  const AttendanceCorrectionCreateDto({
    this.employeeId,
    required this.date,
    required this.punchType,
    required this.correctTime,
    required this.reason,
  });

  /// لمدير القسم فقط عند الرفع نيابة عن موظف من فريقه. null أو غيابه يعني مقدم الطلب نفسه.
  final String? employeeId;
  /// يوم الوردية، ولا يكون في المستقبل.
  final DateTime date;
  final AttendancePunchType punchType;
  /// الوقت الصحيح بتوقيت الشركة. في الوردية الليلية يُفسَّر checkOut قبل وقت البداية على أنه في اليوم التالي.
  final String correctTime;
  final String reason;

  factory AttendanceCorrectionCreateDto.fromJson(Map<String, dynamic> json) => AttendanceCorrectionCreateDto(
        employeeId: json['employeeId'] == null ? null : (json['employeeId']) as String,
        date: hrDateFromJson((json['date']) as String),
        punchType: AttendancePunchType.fromJson(json['punchType']),
        correctTime: (json['correctTime']) as String,
        reason: (json['reason']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (employeeId != null) 'employeeId': employeeId!,
        'date': hrDateToJson(date),
        'punchType': punchType.toJson(),
        'correctTime': correctTime,
        'reason': reason,
      };
}

class AttendanceCorrectionDto {
  const AttendanceCorrectionDto({
    required this.requestId,
    required this.employee,
    required this.submittedBy,
    required this.isOnBehalf,
    required this.date,
    required this.punchType,
    required this.correctTime,
    required this.reason,
    required this.status,
    this.originalValue,
    this.appliedValue,
    this.appliedAt,
    this.affectsClosedMonth,
    this.settlementPeriod,
    required this.creationTime,
  });

  /// معرف الطلب العام. تُستخدم به مسارات الموافقة والرفض والإلغاء في /api/app/request/{id}.
  final String requestId;
  final EmployeeRefDto employee;
  /// مقدم الطلب الفعلي.
  final UserRefDto submittedBy;
  final bool isOnBehalf;
  final DateTime date;
  final AttendancePunchType punchType;
  final String correctTime;
  final String reason;
  final RequestStatus status;
  /// القيمة الحالية في يوم الحضور عند التقديم. null إذا لم تكن هناك بصمة.
  final DateTime? originalValue;
  /// القيمة بعد التطبيق. null قبل الاعتماد النهائي.
  final DateTime? appliedValue;
  final DateTime? appliedAt;
  /// true إذا طُبق التصحيح على شهر مغلق.
  final bool? affectsClosedMonth;
  /// شهر التسوية بصيغة yyyy-MM، دون مبالغ.
  final String? settlementPeriod;
  final DateTime creationTime;

  factory AttendanceCorrectionDto.fromJson(Map<String, dynamic> json) => AttendanceCorrectionDto(
        requestId: (json['requestId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        submittedBy: UserRefDto.fromJson((json['submittedBy']) as Map<String, dynamic>),
        isOnBehalf: (json['isOnBehalf']) as bool,
        date: hrDateFromJson((json['date']) as String),
        punchType: AttendancePunchType.fromJson(json['punchType']),
        correctTime: (json['correctTime']) as String,
        reason: (json['reason']) as String,
        status: RequestStatus.fromJson(json['status']),
        originalValue: json['originalValue'] == null ? null : DateTime.parse((json['originalValue']) as String),
        appliedValue: json['appliedValue'] == null ? null : DateTime.parse((json['appliedValue']) as String),
        appliedAt: json['appliedAt'] == null ? null : DateTime.parse((json['appliedAt']) as String),
        affectsClosedMonth: json['affectsClosedMonth'] == null ? null : (json['affectsClosedMonth']) as bool,
        settlementPeriod: json['settlementPeriod'] == null ? null : (json['settlementPeriod']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'requestId': requestId,
        'employee': employee.toJson(),
        'submittedBy': submittedBy.toJson(),
        'isOnBehalf': isOnBehalf,
        'date': hrDateToJson(date),
        'punchType': punchType.toJson(),
        'correctTime': correctTime,
        'reason': reason,
        'status': status.toJson(),
        if (originalValue != null) 'originalValue': hrDateTimeToJson(originalValue!),
        if (appliedValue != null) 'appliedValue': hrDateTimeToJson(appliedValue!),
        if (appliedAt != null) 'appliedAt': hrDateTimeToJson(appliedAt!),
        if (affectsClosedMonth != null) 'affectsClosedMonth': affectsClosedMonth!,
        if (settlementPeriod != null) 'settlementPeriod': settlementPeriod!,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

/// يُؤخذ الموقع عند التسجيل فقط، ولا يوجد تتبع مستمر. وقت العملية هو وقت الخادم.
class AttendanceGpsPunchCreateDto {
  const AttendanceGpsPunchCreateDto({
    required this.latitude,
    required this.longitude,
    this.accuracyMeters,
    required this.isMockedLocation,
  });

  final double latitude;
  final double longitude;
  final double? accuracyMeters;
  /// ما يبلغ به نظام الجهاز عن موقع مزيف.
  final bool isMockedLocation;

  factory AttendanceGpsPunchCreateDto.fromJson(Map<String, dynamic> json) => AttendanceGpsPunchCreateDto(
        latitude: ((json['latitude']) as num).toDouble(),
        longitude: ((json['longitude']) as num).toDouble(),
        accuracyMeters: json['accuracyMeters'] == null ? null : ((json['accuracyMeters']) as num).toDouble(),
        isMockedLocation: (json['isMockedLocation']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'latitude': latitude,
        'longitude': longitude,
        if (accuracyMeters != null) 'accuracyMeters': accuracyMeters!,
        'isMockedLocation': isMockedLocation,
      };
}

class GeoFenceRefDto {
  const GeoFenceRefDto({
    required this.id,
    required this.name,
  });

  final String id;
  /// Localized by Accept-Language.
  final String name;

  factory GeoFenceRefDto.fromJson(Map<String, dynamic> json) => GeoFenceRefDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
      };
}

/// لا يتضمن علامة الاشتباه، فهي تظهر للموارد البشرية فقط.
class AttendanceGpsPunchResultDto {
  const AttendanceGpsPunchResultDto({
    required this.punchId,
    required this.punchTime,
    required this.geoFence,
  });

  final String punchId;
  /// وقت الخادم.
  final DateTime punchTime;
  final GeoFenceRefDto geoFence;

  factory AttendanceGpsPunchResultDto.fromJson(Map<String, dynamic> json) => AttendanceGpsPunchResultDto(
        punchId: (json['punchId']) as String,
        punchTime: DateTime.parse((json['punchTime']) as String),
        geoFence: GeoFenceRefDto.fromJson((json['geoFence']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'punchId': punchId,
        'punchTime': hrDateTimeToJson(punchTime),
        'geoFence': geoFence.toJson(),
      };
}

enum AttendanceGpsFlagReason {
  mockedLocation('mockedLocation'),
  lowAccuracy('lowAccuracy'),
  impossibleTravel('impossibleTravel'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AttendanceGpsFlagReason(this.value);
  final String value;

  static AttendanceGpsFlagReason fromJson(Object? json) {
    for (final v in AttendanceGpsFlagReason.values) {
      if (v.value == json) return v;
    }
    return AttendanceGpsFlagReason.$unknown;
  }

  String toJson() => value;
}

class AttendanceFlaggedGpsPunchDto {
  const AttendanceFlaggedGpsPunchDto({
    required this.punchId,
    required this.employee,
    required this.punchTime,
    required this.latitude,
    required this.longitude,
    this.accuracyMeters,
    required this.flagReason,
  });

  final String punchId;
  final EmployeeRefDto employee;
  final DateTime punchTime;
  final double latitude;
  final double longitude;
  final double? accuracyMeters;
  final AttendanceGpsFlagReason flagReason;

  factory AttendanceFlaggedGpsPunchDto.fromJson(Map<String, dynamic> json) => AttendanceFlaggedGpsPunchDto(
        punchId: (json['punchId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        punchTime: DateTime.parse((json['punchTime']) as String),
        latitude: ((json['latitude']) as num).toDouble(),
        longitude: ((json['longitude']) as num).toDouble(),
        accuracyMeters: json['accuracyMeters'] == null ? null : ((json['accuracyMeters']) as num).toDouble(),
        flagReason: AttendanceGpsFlagReason.fromJson(json['flagReason']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'punchId': punchId,
        'employee': employee.toJson(),
        'punchTime': hrDateTimeToJson(punchTime),
        'latitude': latitude,
        'longitude': longitude,
        if (accuracyMeters != null) 'accuracyMeters': accuracyMeters!,
        'flagReason': flagReason.toJson(),
      };
}

class AttendanceFlaggedGpsPunchPagedResultDto {
  const AttendanceFlaggedGpsPunchPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<AttendanceFlaggedGpsPunchDto> items;

  factory AttendanceFlaggedGpsPunchPagedResultDto.fromJson(Map<String, dynamic> json) => AttendanceFlaggedGpsPunchPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AttendanceFlaggedGpsPunchDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class GeoFenceCreateDto {
  const GeoFenceCreateDto({
    required this.nameAr,
    required this.nameEn,
    required this.latitude,
    required this.longitude,
    required this.radiusMeters,
  });

  final String nameAr;
  final String nameEn;
  final double latitude;
  final double longitude;
  final int radiusMeters;

  factory GeoFenceCreateDto.fromJson(Map<String, dynamic> json) => GeoFenceCreateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        latitude: ((json['latitude']) as num).toDouble(),
        longitude: ((json['longitude']) as num).toDouble(),
        radiusMeters: ((json['radiusMeters']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'latitude': latitude,
        'longitude': longitude,
        'radiusMeters': radiusMeters,
      };
}

class GeoFenceUpdateDto {
  const GeoFenceUpdateDto({
    required this.nameAr,
    required this.nameEn,
    required this.latitude,
    required this.longitude,
    required this.radiusMeters,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String nameAr;
  final String nameEn;
  final double latitude;
  final double longitude;
  final int radiusMeters;
  final bool isActive;
  final String concurrencyStamp;

  factory GeoFenceUpdateDto.fromJson(Map<String, dynamic> json) => GeoFenceUpdateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        latitude: ((json['latitude']) as num).toDouble(),
        longitude: ((json['longitude']) as num).toDouble(),
        radiusMeters: ((json['radiusMeters']) as num).toInt(),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'latitude': latitude,
        'longitude': longitude,
        'radiusMeters': radiusMeters,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

class GeoFenceDto {
  const GeoFenceDto({
    required this.id,
    required this.name,
    required this.nameAr,
    required this.nameEn,
    required this.latitude,
    required this.longitude,
    required this.radiusMeters,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String id;
  /// Localized by Accept-Language.
  final String name;
  final String nameAr;
  final String nameEn;
  final double latitude;
  final double longitude;
  final int radiusMeters;
  final bool isActive;
  final String concurrencyStamp;

  factory GeoFenceDto.fromJson(Map<String, dynamic> json) => GeoFenceDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        latitude: ((json['latitude']) as num).toDouble(),
        longitude: ((json['longitude']) as num).toDouble(),
        radiusMeters: ((json['radiusMeters']) as num).toInt(),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'latitude': latitude,
        'longitude': longitude,
        'radiusMeters': radiusMeters,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

class GeoFencePagedResultDto {
  const GeoFencePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<GeoFenceDto> items;

  factory GeoFencePagedResultDto.fromJson(Map<String, dynamic> json) => GeoFencePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => GeoFenceDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// hourly = زمنية. annual = من الرصيد السنوي. other = أخرى.
enum LeaveKind {
  hourly('hourly'),
  annual('annual'),
  other('other'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const LeaveKind(this.value);
  final String value;

  static LeaveKind fromJson(Object? json) {
    for (final v in LeaveKind.values) {
      if (v.value == json) return v;
    }
    return LeaveKind.$unknown;
  }

  String toJson() => value;
}

/// grant = منح استحقاق السنة. carryIn = ترحيل من السنة الماضية. hold = حجز عند التقديم. release = فك حجز عند الرفض أو الإلغاء. deduct = خصم عند الاعتماد النهائي. adjust = تعديل يدوي بسبب. reverse = حركة عكسية للتصحيح.
enum LeaveLedgerType {
  grant('grant'),
  carryIn('carryIn'),
  hold('hold'),
  release('release'),
  deduct('deduct'),
  adjust('adjust'),
  reverse('reverse'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const LeaveLedgerType(this.value);
  final String value;

  static LeaveLedgerType fromJson(Object? json) {
    for (final v in LeaveLedgerType.values) {
      if (v.value == json) return v;
    }
    return LeaveLedgerType.$unknown;
  }

  String toJson() => value;
}

/// الوعاء المتأثر: المضاف من السنة الماضية، أو رصيد السنة الحالية، أو رصيد الزمنيات.
enum LeaveLedgerBucket {
  carried('carried'),
  entitlement('entitlement'),
  hourly('hourly'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const LeaveLedgerBucket(this.value);
  final String value;

  static LeaveLedgerBucket fromJson(Object? json) {
    for (final v in LeaveLedgerBucket.values) {
      if (v.value == json) return v;
    }
    return LeaveLedgerBucket.$unknown;
  }

  String toJson() => value;
}

/// أثر الطلب الحالي في الرصيد. none للأنواع الأخرى التي لا تخصم من السنوي.
enum LeaveBalanceEffect {
  none('none'),
  held('held'),
  deducted('deducted'),
  released('released'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const LeaveBalanceEffect(this.value);
  final String value;

  static LeaveBalanceEffect fromJson(Object? json) {
    for (final v in LeaveBalanceEffect.values) {
      if (v.value == json) return v;
    }
    return LeaveBalanceEffect.$unknown;
  }

  String toJson() => value;
}

enum LeaveDayClassification {
  workingDay('workingDay'),
  weeklyOff('weeklyOff'),
  holiday('holiday'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const LeaveDayClassification(this.value);
  final String value;

  static LeaveDayClassification fromJson(Object? json) {
    for (final v in LeaveDayClassification.values) {
      if (v.value == json) return v;
    }
    return LeaveDayClassification.$unknown;
  }

  String toJson() => value;
}

/// الفئة المستحقة. راجع x-open-question في OtherLeaveType_Create.
enum OtherLeaveEligibleCategory {
  all('all'),
  male('male'),
  female('female'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const OtherLeaveEligibleCategory(this.value);
  final String value;

  static OtherLeaveEligibleCategory fromJson(Object? json) {
    for (final v in OtherLeaveEligibleCategory.values) {
      if (v.value == json) return v;
    }
    return OtherLeaveEligibleCategory.$unknown;
  }

  String toJson() => value;
}

class LeaveHourlyBalanceDto {
  const LeaveHourlyBalanceDto({
    required this.isConfigured,
    required this.month,
    required this.quotaHours,
    required this.usedHours,
    required this.heldHours,
    required this.remainingHours,
    this.hoursPerDay,
  });

  /// false إذا لم تُضبط /api/app/system-settings/hourly-leave بعد، وعندها تكون الساعات 0 وتُرفض الطلبات الزمنية.
  final bool isConfigured;
  /// الشهر التقويمي المعروض بصيغة yyyy-MM بتوقيت الشركة.
  final String month;
  /// الساعات المسموحة في الشهر.
  final double quotaHours;
  /// الساعات المخصومة من حصة هذا الشهر بعد الاعتماد النهائي.
  final double usedHours;
  /// الساعات المحجوزة من حصة هذا الشهر لطلبات معلقة.
  final double heldHours;
  /// quotaHours - usedHours - heldHours.
  final double remainingHours;
  /// عدد الساعات التي تساوي يومًا عند تحويل الزيادة إلى الرصيد السنوي.
  final double? hoursPerDay;

  factory LeaveHourlyBalanceDto.fromJson(Map<String, dynamic> json) => LeaveHourlyBalanceDto(
        isConfigured: (json['isConfigured']) as bool,
        month: (json['month']) as String,
        quotaHours: ((json['quotaHours']) as num).toDouble(),
        usedHours: ((json['usedHours']) as num).toDouble(),
        heldHours: ((json['heldHours']) as num).toDouble(),
        remainingHours: ((json['remainingHours']) as num).toDouble(),
        hoursPerDay: json['hoursPerDay'] == null ? null : ((json['hoursPerDay']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'isConfigured': isConfigured,
        'month': month,
        'quotaHours': quotaHours,
        'usedHours': usedHours,
        'heldHours': heldHours,
        'remainingHours': remainingHours,
        if (hoursPerDay != null) 'hoursPerDay': hoursPerDay!,
      };
}

class OtherLeaveUsageDto {
  const OtherLeaveUsageDto({
    required this.leaveTypeId,
    required this.leaveTypeName,
    required this.usedDays,
    required this.heldDays,
    this.maxDays,
    this.remainingDays,
  });

  final String leaveTypeId;
  /// Localized by Accept-Language.
  final String leaveTypeName;
  /// داخل سنة الموظف الحالية.
  final double usedDays;
  final double heldDays;
  /// null يعني بلا حد.
  final double? maxDays;
  /// null إذا لم يكن هناك حد.
  final double? remainingDays;

  factory OtherLeaveUsageDto.fromJson(Map<String, dynamic> json) => OtherLeaveUsageDto(
        leaveTypeId: (json['leaveTypeId']) as String,
        leaveTypeName: (json['leaveTypeName']) as String,
        usedDays: ((json['usedDays']) as num).toDouble(),
        heldDays: ((json['heldDays']) as num).toDouble(),
        maxDays: json['maxDays'] == null ? null : ((json['maxDays']) as num).toDouble(),
        remainingDays: json['remainingDays'] == null ? null : ((json['remainingDays']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'leaveTypeId': leaveTypeId,
        'leaveTypeName': leaveTypeName,
        'usedDays': usedDays,
        'heldDays': heldDays,
        if (maxDays != null) 'maxDays': maxDays!,
        if (remainingDays != null) 'remainingDays': remainingDays!,
      };
}

/// لا يوجد هذا الكائن للمدير التنفيذي مطلقًا.
class LeaveBalanceDto {
  const LeaveBalanceDto({
    required this.employee,
    required this.isEntitlementSet,
    this.leaveYearId,
    required this.yearStartDate,
    required this.yearEndDate,
    required this.nextYearStartDate,
    required this.currentYearEntitlementDays,
    required this.carriedDays,
    required this.adjustmentDays,
    required this.usedDays,
    required this.heldDays,
    required this.remainingDays,
    required this.carriedRemainingDays,
    required this.currentYearRemainingDays,
    required this.futureYearHeldDays,
    this.nextYearEntitlementDays,
    required this.hourly,
    required this.otherLeaveUsages,
    required this.concurrencyStamp,
  });

  final EmployeeRefDto employee;
  /// false إذا لم يحدد مدير الموارد البشرية عدد الأيام بعد. عندها كل قيم الأيام 0 وتُرفض طلبات الرصيد بـHr:Leave:EntitlementNotSet.
  final bool isEntitlementSet;
  /// null فقط إذا لم تُفتح سنة بعد، أي عندما isEntitlementSet = false.
  final String? leaveYearId;
  /// بداية سنة الموظف الحالية، من تاريخ مباشرته وليس من السنة التقويمية. من باشر في 29 فبراير تبدأ سنته في 28 فبراير إذا لم تكن السنة كبيسة.
  final DateTime yearStartDate;
  /// آخر يوم في السنة الحالية، أي اليوم السابق لـnextYearStartDate.
  final DateTime yearEndDate;
  /// بداية سنة الموظف القادمة، بقاعدة 29 فبراير نفسها.
  final DateTime nextYearStartDate;
  /// استحقاق السنة الحالية كما حدده مدير الموارد البشرية.
  final double currentYearEntitlementDays;
  /// ما رُحِّل كاملًا من السنة السابقة. تسميته في الواجهة: «الإجازات المضافة من السنة الماضية» / Added from last year.
  final double carriedDays;
  /// صافي تعديلات مدير الموارد البشرية في السنة الحالية، وقد يكون سالبًا.
  final double adjustmentDays;
  /// المخصوم نهائيًا في السنة الحالية، ويشمل زيادة الزمنيات المحولة إلى أيام.
  final double usedDays;
  /// المحجوز لطلبات معلقة على السنة الحالية.
  final double heldDays;
  /// المتاح الآن = currentYearEntitlementDays + carriedDays + adjustmentDays - usedDays - heldDays. لا يكون سالبًا.
  final double remainingDays;
  /// المتبقي من الأيام المضافة، وهي تُخصم أولًا.
  final double carriedRemainingDays;
  /// المتبقي من رصيد السنة الحالية بعد التعديلات. remainingDays = carriedRemainingDays + currentYearRemainingDays.
  final double currentYearRemainingDays;
  /// أيام محجوزة على سنة الموظف القادمة من إجازات ممتدة أو واقعة فيها، ولا تدخل في remainingDays.
  final double futureYearHeldDays;
  /// استحقاق السنة القادمة إذا غيّره مدير الموارد البشرية، ويساوي الاستحقاق الحالي إن لم يتغير.
  final double? nextYearEntitlementDays;
  final LeaveHourlyBalanceDto hourly;
  /// استخدام الأنواع الأخرى النشطة في السنة الحالية.
  final List<OtherLeaveUsageDto> otherLeaveUsages;
  /// مطلوب عند تحديد الاستحقاق.
  final String concurrencyStamp;

  factory LeaveBalanceDto.fromJson(Map<String, dynamic> json) => LeaveBalanceDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        isEntitlementSet: (json['isEntitlementSet']) as bool,
        leaveYearId: json['leaveYearId'] == null ? null : (json['leaveYearId']) as String,
        yearStartDate: hrDateFromJson((json['yearStartDate']) as String),
        yearEndDate: hrDateFromJson((json['yearEndDate']) as String),
        nextYearStartDate: hrDateFromJson((json['nextYearStartDate']) as String),
        currentYearEntitlementDays: ((json['currentYearEntitlementDays']) as num).toDouble(),
        carriedDays: ((json['carriedDays']) as num).toDouble(),
        adjustmentDays: ((json['adjustmentDays']) as num).toDouble(),
        usedDays: ((json['usedDays']) as num).toDouble(),
        heldDays: ((json['heldDays']) as num).toDouble(),
        remainingDays: ((json['remainingDays']) as num).toDouble(),
        carriedRemainingDays: ((json['carriedRemainingDays']) as num).toDouble(),
        currentYearRemainingDays: ((json['currentYearRemainingDays']) as num).toDouble(),
        futureYearHeldDays: ((json['futureYearHeldDays']) as num).toDouble(),
        nextYearEntitlementDays: json['nextYearEntitlementDays'] == null ? null : ((json['nextYearEntitlementDays']) as num).toDouble(),
        hourly: LeaveHourlyBalanceDto.fromJson((json['hourly']) as Map<String, dynamic>),
        otherLeaveUsages: ((json['otherLeaveUsages']) as List).map((e) => OtherLeaveUsageDto.fromJson((e) as Map<String, dynamic>)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'isEntitlementSet': isEntitlementSet,
        if (leaveYearId != null) 'leaveYearId': leaveYearId!,
        'yearStartDate': hrDateToJson(yearStartDate),
        'yearEndDate': hrDateToJson(yearEndDate),
        'nextYearStartDate': hrDateToJson(nextYearStartDate),
        'currentYearEntitlementDays': currentYearEntitlementDays,
        'carriedDays': carriedDays,
        'adjustmentDays': adjustmentDays,
        'usedDays': usedDays,
        'heldDays': heldDays,
        'remainingDays': remainingDays,
        'carriedRemainingDays': carriedRemainingDays,
        'currentYearRemainingDays': currentYearRemainingDays,
        'futureYearHeldDays': futureYearHeldDays,
        if (nextYearEntitlementDays != null) 'nextYearEntitlementDays': nextYearEntitlementDays!,
        'hourly': hourly.toJson(),
        'otherLeaveUsages': otherLeaveUsages.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
      };
}

class LeaveEntitlementUpdateDto {
  const LeaveEntitlementUpdateDto({
    required this.entitlementDays,
    this.reason,
    required this.concurrencyStamp,
  });

  /// عدد الأيام، مثل 21 أو 24.
  final double entitlementDays;
  /// سبب اختياري يُحفظ في الحركة.
  final String? reason;
  final String concurrencyStamp;

  factory LeaveEntitlementUpdateDto.fromJson(Map<String, dynamic> json) => LeaveEntitlementUpdateDto(
        entitlementDays: ((json['entitlementDays']) as num).toDouble(),
        reason: json['reason'] == null ? null : (json['reason']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'entitlementDays': entitlementDays,
        if (reason != null) 'reason': reason!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class LeaveBalanceAdjustDto {
  const LeaveBalanceAdjustDto({
    required this.days,
    required this.reason,
  });

  /// موجب للإضافة، سالب للخصم، ولا يكون صفرًا. يطبق على السنة الحالية.
  final double days;
  /// السبب إلزامي.
  final String reason;

  factory LeaveBalanceAdjustDto.fromJson(Map<String, dynamic> json) => LeaveBalanceAdjustDto(
        days: ((json['days']) as num).toDouble(),
        reason: (json['reason']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'days': days,
        'reason': reason,
      };
}

/// حركة لا تُعدَّل ولا تُحذف، والتصحيح بحركة عكسية.
class LeaveLedgerEntryDto {
  const LeaveLedgerEntryDto({
    required this.id,
    required this.employeeId,
    required this.leaveYearId,
    required this.yearStartDate,
    required this.yearEndDate,
    required this.type,
    required this.bucket,
    this.days,
    this.hours,
    this.requestId,
    this.reason,
    this.createdBy,
    required this.creationTime,
  });

  final String id;
  final String employeeId;
  final String leaveYearId;
  final DateTime yearStartDate;
  final DateTime yearEndDate;
  final LeaveLedgerType type;
  final LeaveLedgerBucket bucket;
  /// بإشارة: موجب يزيد المتاح وسالب ينقصه. null لحركات الساعات.
  final double? days;
  /// بإشارة، لوعاء الزمنيات فقط. null لغيره.
  final double? hours;
  /// الطلب العام المرتبط بالحركة.
  final String? requestId;
  /// إلزامي في adjust.
  final String? reason;
  /// null للحركات التي ينفذها النظام، مثل الترحيل.
  final UserRefDto? createdBy;
  final DateTime creationTime;

  factory LeaveLedgerEntryDto.fromJson(Map<String, dynamic> json) => LeaveLedgerEntryDto(
        id: (json['id']) as String,
        employeeId: (json['employeeId']) as String,
        leaveYearId: (json['leaveYearId']) as String,
        yearStartDate: hrDateFromJson((json['yearStartDate']) as String),
        yearEndDate: hrDateFromJson((json['yearEndDate']) as String),
        type: LeaveLedgerType.fromJson(json['type']),
        bucket: LeaveLedgerBucket.fromJson(json['bucket']),
        days: json['days'] == null ? null : ((json['days']) as num).toDouble(),
        hours: json['hours'] == null ? null : ((json['hours']) as num).toDouble(),
        requestId: json['requestId'] == null ? null : (json['requestId']) as String,
        reason: json['reason'] == null ? null : (json['reason']) as String,
        createdBy: json['createdBy'] == null ? null : UserRefDto.fromJson((json['createdBy']) as Map<String, dynamic>),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employeeId': employeeId,
        'leaveYearId': leaveYearId,
        'yearStartDate': hrDateToJson(yearStartDate),
        'yearEndDate': hrDateToJson(yearEndDate),
        'type': type.toJson(),
        'bucket': bucket.toJson(),
        if (days != null) 'days': days!,
        if (hours != null) 'hours': hours!,
        if (requestId != null) 'requestId': requestId!,
        if (reason != null) 'reason': reason!,
        if (createdBy != null) 'createdBy': createdBy!.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class LeaveLedgerEntryPagedResultDto {
  const LeaveLedgerEntryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<LeaveLedgerEntryDto> items;

  factory LeaveLedgerEntryPagedResultDto.fromJson(Map<String, dynamic> json) => LeaveLedgerEntryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => LeaveLedgerEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class OtherLeaveTypeCreateDto {
  const OtherLeaveTypeCreateDto({
    required this.nameAr,
    required this.nameEn,
    required this.isPaid,
    this.maxDays,
    required this.requiresAttachment,
    required this.eligibleCategory,
    this.deductsFromAnnualBalance,
    required this.isActive,
  });

  final String nameAr;
  final String nameEn;
  final bool isPaid;
  /// الحد الأقصى داخل سنة الموظف. null يعني بلا حد.
  final double? maxDays;
  final bool requiresAttachment;
  final OtherLeaveEligibleCategory eligibleCategory;
  /// false افتراضيًا. إذا كان true يطبق ترتيب الخصم والحجز على الرصيد السنوي.
  final bool? deductsFromAnnualBalance;
  final bool isActive;

  factory OtherLeaveTypeCreateDto.fromJson(Map<String, dynamic> json) => OtherLeaveTypeCreateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        isPaid: (json['isPaid']) as bool,
        maxDays: json['maxDays'] == null ? null : ((json['maxDays']) as num).toDouble(),
        requiresAttachment: (json['requiresAttachment']) as bool,
        eligibleCategory: OtherLeaveEligibleCategory.fromJson(json['eligibleCategory']),
        deductsFromAnnualBalance: json['deductsFromAnnualBalance'] == null ? null : (json['deductsFromAnnualBalance']) as bool,
        isActive: (json['isActive']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'isPaid': isPaid,
        if (maxDays != null) 'maxDays': maxDays!,
        'requiresAttachment': requiresAttachment,
        'eligibleCategory': eligibleCategory.toJson(),
        if (deductsFromAnnualBalance != null) 'deductsFromAnnualBalance': deductsFromAnnualBalance!,
        'isActive': isActive,
      };
}

class OtherLeaveTypeUpdateDto {
  const OtherLeaveTypeUpdateDto({
    required this.nameAr,
    required this.nameEn,
    required this.isPaid,
    this.maxDays,
    required this.requiresAttachment,
    required this.eligibleCategory,
    required this.deductsFromAnnualBalance,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String nameAr;
  final String nameEn;
  final bool isPaid;
  /// الحد الأقصى داخل سنة الموظف. null يعني بلا حد.
  final double? maxDays;
  final bool requiresAttachment;
  final OtherLeaveEligibleCategory eligibleCategory;
  /// false افتراضيًا. إذا كان true يطبق ترتيب الخصم والحجز على الرصيد السنوي.
  final bool deductsFromAnnualBalance;
  final bool isActive;
  final String concurrencyStamp;

  factory OtherLeaveTypeUpdateDto.fromJson(Map<String, dynamic> json) => OtherLeaveTypeUpdateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        isPaid: (json['isPaid']) as bool,
        maxDays: json['maxDays'] == null ? null : ((json['maxDays']) as num).toDouble(),
        requiresAttachment: (json['requiresAttachment']) as bool,
        eligibleCategory: OtherLeaveEligibleCategory.fromJson(json['eligibleCategory']),
        deductsFromAnnualBalance: (json['deductsFromAnnualBalance']) as bool,
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'isPaid': isPaid,
        if (maxDays != null) 'maxDays': maxDays!,
        'requiresAttachment': requiresAttachment,
        'eligibleCategory': eligibleCategory.toJson(),
        'deductsFromAnnualBalance': deductsFromAnnualBalance,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// عرض مسؤول النظام.
class OtherLeaveTypeDto {
  const OtherLeaveTypeDto({
    required this.id,
    required this.name,
    required this.nameAr,
    required this.nameEn,
    required this.isPaid,
    this.maxDays,
    required this.requiresAttachment,
    required this.eligibleCategory,
    required this.deductsFromAnnualBalance,
    required this.isActive,
    required this.creationTime,
    required this.concurrencyStamp,
  });

  final String id;
  /// Localized by Accept-Language.
  final String name;
  final String nameAr;
  final String nameEn;
  final bool isPaid;
  /// الحد الأقصى داخل سنة الموظف. null يعني بلا حد.
  final double? maxDays;
  final bool requiresAttachment;
  final OtherLeaveEligibleCategory eligibleCategory;
  /// false افتراضيًا. إذا كان true يطبق ترتيب الخصم والحجز على الرصيد السنوي.
  final bool deductsFromAnnualBalance;
  final bool isActive;
  final DateTime creationTime;
  final String concurrencyStamp;

  factory OtherLeaveTypeDto.fromJson(Map<String, dynamic> json) => OtherLeaveTypeDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        isPaid: (json['isPaid']) as bool,
        maxDays: json['maxDays'] == null ? null : ((json['maxDays']) as num).toDouble(),
        requiresAttachment: (json['requiresAttachment']) as bool,
        eligibleCategory: OtherLeaveEligibleCategory.fromJson(json['eligibleCategory']),
        deductsFromAnnualBalance: (json['deductsFromAnnualBalance']) as bool,
        isActive: (json['isActive']) as bool,
        creationTime: DateTime.parse((json['creationTime']) as String),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'isPaid': isPaid,
        if (maxDays != null) 'maxDays': maxDays!,
        'requiresAttachment': requiresAttachment,
        'eligibleCategory': eligibleCategory.toJson(),
        'deductsFromAnnualBalance': deductsFromAnnualBalance,
        'isActive': isActive,
        'creationTime': hrDateTimeToJson(creationTime),
        'concurrencyStamp': concurrencyStamp,
      };
}

class OtherLeaveTypePagedResultDto {
  const OtherLeaveTypePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<OtherLeaveTypeDto> items;

  factory OtherLeaveTypePagedResultDto.fromJson(Map<String, dynamic> json) => OtherLeaveTypePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => OtherLeaveTypeDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// عرض الموظف عند اختيار النوع.
class OtherLeaveTypeOptionDto {
  const OtherLeaveTypeOptionDto({
    required this.id,
    required this.name,
    required this.isPaid,
    this.maxDays,
    required this.requiresAttachment,
    required this.deductsFromAnnualBalance,
  });

  final String id;
  /// Localized by Accept-Language.
  final String name;
  final bool isPaid;
  final double? maxDays;
  final bool requiresAttachment;
  final bool deductsFromAnnualBalance;

  factory OtherLeaveTypeOptionDto.fromJson(Map<String, dynamic> json) => OtherLeaveTypeOptionDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        isPaid: (json['isPaid']) as bool,
        maxDays: json['maxDays'] == null ? null : ((json['maxDays']) as num).toDouble(),
        requiresAttachment: (json['requiresAttachment']) as bool,
        deductsFromAnnualBalance: (json['deductsFromAnnualBalance']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'isPaid': isPaid,
        if (maxDays != null) 'maxDays': maxDays!,
        'requiresAttachment': requiresAttachment,
        'deductsFromAnnualBalance': deductsFromAnnualBalance,
      };
}

class OtherLeaveTypeOptionListDto {
  const OtherLeaveTypeOptionListDto({
    required this.items,
  });

  final List<OtherLeaveTypeOptionDto> items;

  factory OtherLeaveTypeOptionListDto.fromJson(Map<String, dynamic> json) => OtherLeaveTypeOptionListDto(
        items: ((json['items']) as List).map((e) => OtherLeaveTypeOptionDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// الحقول الشرطية حسب kind: مخالفتها تعطي 400 مع validationErrors.
class LeaveRequestPreviewInputDto {
  const LeaveRequestPreviewInputDto({
    this.employeeId,
    required this.kind,
    this.date,
    this.fromTime,
    this.toTime,
    this.startDate,
    this.endDate,
    this.otherLeaveTypeId,
  });

  /// لمدير القسم فقط عند الرفع نيابة عن موظف من فريقه. null أو غيابه يعني مقدم الطلب نفسه.
  final String? employeeId;
  final LeaveKind kind;
  /// إلزامي عندما kind = hourly، ويجب أن يكون null لغيره. هو يوم الوردية.
  final DateTime? date;
  /// إلزامي عندما kind = hourly، ويجب أن يكون null لغيره.
  final String? fromTime;
  /// إلزامي عندما kind = hourly، ويجب أن يكون null لغيره. يكون بعد fromTime، إلا في وردية تعبر منتصف الليل، وغير ذلك يعطي Hr:Leave:InvalidDateRange.
  final String? toTime;
  /// إلزامي عندما kind = annual أو other، ويجب أن يكون null للزمنية.
  final DateTime? startDate;
  /// إلزامي عندما kind = annual أو other، ويجب أن يكون null للزمنية. إذا سبق startDate يعطي Hr:Leave:InvalidDateRange.
  final DateTime? endDate;
  /// إلزامي عندما kind = other، ويجب أن يكون null لغيره.
  final String? otherLeaveTypeId;

  factory LeaveRequestPreviewInputDto.fromJson(Map<String, dynamic> json) => LeaveRequestPreviewInputDto(
        employeeId: json['employeeId'] == null ? null : (json['employeeId']) as String,
        kind: LeaveKind.fromJson(json['kind']),
        date: json['date'] == null ? null : hrDateFromJson((json['date']) as String),
        fromTime: json['fromTime'] == null ? null : (json['fromTime']) as String,
        toTime: json['toTime'] == null ? null : (json['toTime']) as String,
        startDate: json['startDate'] == null ? null : hrDateFromJson((json['startDate']) as String),
        endDate: json['endDate'] == null ? null : hrDateFromJson((json['endDate']) as String),
        otherLeaveTypeId: json['otherLeaveTypeId'] == null ? null : (json['otherLeaveTypeId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (employeeId != null) 'employeeId': employeeId!,
        'kind': kind.toJson(),
        if (date != null) 'date': hrDateToJson(date!),
        if (fromTime != null) 'fromTime': fromTime!,
        if (toTime != null) 'toTime': toTime!,
        if (startDate != null) 'startDate': hrDateToJson(startDate!),
        if (endDate != null) 'endDate': hrDateToJson(endDate!),
        if (otherLeaveTypeId != null) 'otherLeaveTypeId': otherLeaveTypeId!,
      };
}

/// الحقول الشرطية حسب kind: مخالفتها تعطي 400 مع validationErrors.
class LeaveRequestCreateDto {
  const LeaveRequestCreateDto({
    this.employeeId,
    required this.kind,
    this.date,
    this.fromTime,
    this.toTime,
    this.startDate,
    this.endDate,
    this.otherLeaveTypeId,
    this.attachmentIds,
    this.notes,
  });

  /// لمدير القسم فقط عند الرفع نيابة عن موظف من فريقه. null أو غيابه يعني مقدم الطلب نفسه.
  final String? employeeId;
  final LeaveKind kind;
  /// إلزامي عندما kind = hourly، ويجب أن يكون null لغيره. هو يوم الوردية.
  final DateTime? date;
  /// إلزامي عندما kind = hourly، ويجب أن يكون null لغيره.
  final String? fromTime;
  /// إلزامي عندما kind = hourly، ويجب أن يكون null لغيره. يكون بعد fromTime، إلا في وردية تعبر منتصف الليل، وغير ذلك يعطي Hr:Leave:InvalidDateRange.
  final String? toTime;
  /// إلزامي عندما kind = annual أو other، ويجب أن يكون null للزمنية.
  final DateTime? startDate;
  /// إلزامي عندما kind = annual أو other، ويجب أن يكون null للزمنية. إذا سبق startDate يعطي Hr:Leave:InvalidDateRange.
  final DateTime? endDate;
  /// إلزامي عندما kind = other، ويجب أن يكون null لغيره.
  final String? otherLeaveTypeId;
  /// مرفقات رُفعت عبر POST /api/app/leave-request/attachment. إلزامية إذا كان النوع يتطلب مرفقًا.
  final List<String>? attachmentIds;
  /// ملاحظات مقدم الطلب.
  final String? notes;

  factory LeaveRequestCreateDto.fromJson(Map<String, dynamic> json) => LeaveRequestCreateDto(
        employeeId: json['employeeId'] == null ? null : (json['employeeId']) as String,
        kind: LeaveKind.fromJson(json['kind']),
        date: json['date'] == null ? null : hrDateFromJson((json['date']) as String),
        fromTime: json['fromTime'] == null ? null : (json['fromTime']) as String,
        toTime: json['toTime'] == null ? null : (json['toTime']) as String,
        startDate: json['startDate'] == null ? null : hrDateFromJson((json['startDate']) as String),
        endDate: json['endDate'] == null ? null : hrDateFromJson((json['endDate']) as String),
        otherLeaveTypeId: json['otherLeaveTypeId'] == null ? null : (json['otherLeaveTypeId']) as String,
        attachmentIds: json['attachmentIds'] == null ? null : ((json['attachmentIds']) as List).map((e) => (e) as String).toList(),
        notes: json['notes'] == null ? null : (json['notes']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (employeeId != null) 'employeeId': employeeId!,
        'kind': kind.toJson(),
        if (date != null) 'date': hrDateToJson(date!),
        if (fromTime != null) 'fromTime': fromTime!,
        if (toTime != null) 'toTime': toTime!,
        if (startDate != null) 'startDate': hrDateToJson(startDate!),
        if (endDate != null) 'endDate': hrDateToJson(endDate!),
        if (otherLeaveTypeId != null) 'otherLeaveTypeId': otherLeaveTypeId!,
        if (attachmentIds != null) 'attachmentIds': attachmentIds!,
        if (notes != null) 'notes': notes!,
      };
}

/// جزء الإجازة داخل سنة واحدة من سنوات الموظف. الإجازة الممتدة بين سنتين لها جزءان.
class LeaveYearSegmentDto {
  const LeaveYearSegmentDto({
    required this.yearStartDate,
    required this.yearEndDate,
    required this.isFutureYear,
    this.startDate,
    this.endDate,
    required this.workingDays,
    required this.fromCarriedDays,
    required this.fromEntitlementDays,
  });

  /// بداية سنة الموظف التي يقع فيها هذا الجزء.
  final DateTime yearStartDate;
  final DateTime yearEndDate;
  /// true إذا كان الجزء في سنة الموظف القادمة.
  final bool isFutureYear;
  /// أول يوم من الإجازة داخل هذه السنة. null للزمنية.
  final DateTime? startDate;
  /// آخر يوم من الإجازة داخل هذه السنة. null للزمنية.
  final DateTime? endDate;
  /// الأيام المحسوبة في هذا الجزء.
  final double workingDays;
  /// الجزء المحسوب من الأيام المضافة، وهو يُستهلك أولًا.
  final double fromCarriedDays;
  /// الجزء المحسوب من رصيد السنة بعد نفاد الأيام المضافة.
  final double fromEntitlementDays;

  factory LeaveYearSegmentDto.fromJson(Map<String, dynamic> json) => LeaveYearSegmentDto(
        yearStartDate: hrDateFromJson((json['yearStartDate']) as String),
        yearEndDate: hrDateFromJson((json['yearEndDate']) as String),
        isFutureYear: (json['isFutureYear']) as bool,
        startDate: json['startDate'] == null ? null : hrDateFromJson((json['startDate']) as String),
        endDate: json['endDate'] == null ? null : hrDateFromJson((json['endDate']) as String),
        workingDays: ((json['workingDays']) as num).toDouble(),
        fromCarriedDays: ((json['fromCarriedDays']) as num).toDouble(),
        fromEntitlementDays: ((json['fromEntitlementDays']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'yearStartDate': hrDateToJson(yearStartDate),
        'yearEndDate': hrDateToJson(yearEndDate),
        'isFutureYear': isFutureYear,
        if (startDate != null) 'startDate': hrDateToJson(startDate!),
        if (endDate != null) 'endDate': hrDateToJson(endDate!),
        'workingDays': workingDays,
        'fromCarriedDays': fromCarriedDays,
        'fromEntitlementDays': fromEntitlementDays,
      };
}

class LeaveDayBreakdownDto {
  const LeaveDayBreakdownDto({
    required this.date,
    required this.classification,
    this.holidayName,
    required this.isCounted,
  });

  final DateTime date;
  final LeaveDayClassification classification;
  /// Localized. للعطلة الرسمية فقط.
  final String? holidayName;
  /// true فقط لـworkingDay.
  final bool isCounted;

  factory LeaveDayBreakdownDto.fromJson(Map<String, dynamic> json) => LeaveDayBreakdownDto(
        date: hrDateFromJson((json['date']) as String),
        classification: LeaveDayClassification.fromJson(json['classification']),
        holidayName: json['holidayName'] == null ? null : (json['holidayName']) as String,
        isCounted: (json['isCounted']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'date': hrDateToJson(date),
        'classification': classification.toJson(),
        if (holidayName != null) 'holidayName': holidayName!,
        'isCounted': isCounted,
      };
}

class LeavePreviewIssueDto {
  const LeavePreviewIssueDto({
    required this.code,
    required this.message,
  });

  /// أحد أكواد x-error-codes لعملية LeaveRequest_Create، مثل Hr:Leave:InsufficientBalance.
  final String code;
  /// Localized by Accept-Language.
  final String message;

  factory LeavePreviewIssueDto.fromJson(Map<String, dynamic> json) => LeavePreviewIssueDto(
        code: (json['code']) as String,
        message: (json['message']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'code': code,
        'message': message,
      };
}

class LeaveRequestPreviewDto {
  const LeaveRequestPreviewDto({
    required this.kind,
    required this.calendarDays,
    required this.workingDays,
    required this.excludedWeeklyOffDays,
    required this.excludedHolidayDays,
    this.requestedHours,
    this.hourlyQuotaHoursUsed,
    this.hourlyExcessHours,
    required this.deductsFromAnnualBalance,
    required this.annualDays,
    required this.fromCarriedDays,
    required this.fromEntitlementDays,
    this.isPaid,
    required this.requiresAttachment,
    required this.segments,
    required this.dayBreakdown,
    required this.availableDaysBefore,
    required this.remainingDaysAfter,
    this.availableHourlyHoursBefore,
    required this.isBalanceSufficient,
    required this.shortageDays,
    required this.canSubmit,
    required this.issues,
  });

  final LeaveKind kind;
  /// عدد الأيام التقويمية في النطاق. 1 للزمنية.
  final int calendarDays;
  /// أيام دوام الموظف فقط حسب ورديته. 0 للزمنية.
  final double workingDays;
  final int excludedWeeklyOffDays;
  final int excludedHolidayDays;
  /// للزمنية فقط = toTime - fromTime.
  final double? requestedHours;
  /// للزمنية: ما يُحسب من حصة الشهر.
  final double? hourlyQuotaHoursUsed;
  /// للزمنية: الزيادة التي تُحسب من الرصيد السنوي.
  final double? hourlyExcessHours;
  final bool deductsFromAnnualBalance;
  /// إجمالي ما يُحسب من الرصيد السنوي. للزمنية = hourlyExcessHours / hoursPerDay. 0 للأنواع التي لا تخصم.
  final double annualDays;
  final double fromCarriedDays;
  final double fromEntitlementDays;
  /// للنوع other فقط.
  final bool? isPaid;
  final bool requiresAttachment;
  final List<LeaveYearSegmentDto> segments;
  /// تصنيف كل يوم في النطاق، لعرض سبب استبعاده.
  final List<LeaveDayBreakdownDto> dayBreakdown;
  /// المتاح قبل الطلب للسنوات المعنية، شاملًا ما يُتوقع ترحيله إلى السنة القادمة.
  final double availableDaysBefore;
  /// المتبقي بعد الطلب. لا يكون سالبًا، وإذا لم يكف الرصيد يكون 0 وتظهر الزيادة في shortageDays.
  final double remainingDaysAfter;
  /// للزمنية: المتبقي من حصة الشهر قبل الطلب.
  final double? availableHourlyHoursBefore;
  final bool isBalanceSufficient;
  /// 0 إذا كان الرصيد كافيًا.
  final double shortageDays;
  /// true إذا كانت issues فارغة. لا يشمل فحص المرفق، فالمعاينة لا تستقبل مرفقات.
  final bool canSubmit;
  /// كل ما سيمنع التقديم لو أُرسل الطلب الآن.
  final List<LeavePreviewIssueDto> issues;

  factory LeaveRequestPreviewDto.fromJson(Map<String, dynamic> json) => LeaveRequestPreviewDto(
        kind: LeaveKind.fromJson(json['kind']),
        calendarDays: ((json['calendarDays']) as num).toInt(),
        workingDays: ((json['workingDays']) as num).toDouble(),
        excludedWeeklyOffDays: ((json['excludedWeeklyOffDays']) as num).toInt(),
        excludedHolidayDays: ((json['excludedHolidayDays']) as num).toInt(),
        requestedHours: json['requestedHours'] == null ? null : ((json['requestedHours']) as num).toDouble(),
        hourlyQuotaHoursUsed: json['hourlyQuotaHoursUsed'] == null ? null : ((json['hourlyQuotaHoursUsed']) as num).toDouble(),
        hourlyExcessHours: json['hourlyExcessHours'] == null ? null : ((json['hourlyExcessHours']) as num).toDouble(),
        deductsFromAnnualBalance: (json['deductsFromAnnualBalance']) as bool,
        annualDays: ((json['annualDays']) as num).toDouble(),
        fromCarriedDays: ((json['fromCarriedDays']) as num).toDouble(),
        fromEntitlementDays: ((json['fromEntitlementDays']) as num).toDouble(),
        isPaid: json['isPaid'] == null ? null : (json['isPaid']) as bool,
        requiresAttachment: (json['requiresAttachment']) as bool,
        segments: ((json['segments']) as List).map((e) => LeaveYearSegmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
        dayBreakdown: ((json['dayBreakdown']) as List).map((e) => LeaveDayBreakdownDto.fromJson((e) as Map<String, dynamic>)).toList(),
        availableDaysBefore: ((json['availableDaysBefore']) as num).toDouble(),
        remainingDaysAfter: ((json['remainingDaysAfter']) as num).toDouble(),
        availableHourlyHoursBefore: json['availableHourlyHoursBefore'] == null ? null : ((json['availableHourlyHoursBefore']) as num).toDouble(),
        isBalanceSufficient: (json['isBalanceSufficient']) as bool,
        shortageDays: ((json['shortageDays']) as num).toDouble(),
        canSubmit: (json['canSubmit']) as bool,
        issues: ((json['issues']) as List).map((e) => LeavePreviewIssueDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'kind': kind.toJson(),
        'calendarDays': calendarDays,
        'workingDays': workingDays,
        'excludedWeeklyOffDays': excludedWeeklyOffDays,
        'excludedHolidayDays': excludedHolidayDays,
        if (requestedHours != null) 'requestedHours': requestedHours!,
        if (hourlyQuotaHoursUsed != null) 'hourlyQuotaHoursUsed': hourlyQuotaHoursUsed!,
        if (hourlyExcessHours != null) 'hourlyExcessHours': hourlyExcessHours!,
        'deductsFromAnnualBalance': deductsFromAnnualBalance,
        'annualDays': annualDays,
        'fromCarriedDays': fromCarriedDays,
        'fromEntitlementDays': fromEntitlementDays,
        if (isPaid != null) 'isPaid': isPaid!,
        'requiresAttachment': requiresAttachment,
        'segments': segments.map((e) => e.toJson()).toList(),
        'dayBreakdown': dayBreakdown.map((e) => e.toJson()).toList(),
        'availableDaysBefore': availableDaysBefore,
        'remainingDaysAfter': remainingDaysAfter,
        if (availableHourlyHoursBefore != null) 'availableHourlyHoursBefore': availableHourlyHoursBefore!,
        'isBalanceSufficient': isBalanceSufficient,
        'shortageDays': shortageDays,
        'canSubmit': canSubmit,
        'issues': issues.map((e) => e.toJson()).toList(),
      };
}

class LeaveRequestDto {
  const LeaveRequestDto({
    required this.requestId,
    required this.employee,
    required this.submittedBy,
    required this.isOnBehalf,
    required this.kind,
    this.date,
    this.fromTime,
    this.toTime,
    this.startDate,
    this.endDate,
    this.otherLeaveType,
    this.notes,
    required this.status,
    required this.balanceEffect,
    required this.workingDays,
    this.requestedHours,
    this.hourlyQuotaHoursUsed,
    this.hourlyExcessHours,
    required this.annualDays,
    required this.fromCarriedDays,
    required this.fromEntitlementDays,
    required this.segments,
    required this.attachments,
    required this.creationTime,
  });

  /// معرف الطلب العام نفسه. لا يوجد معرف منفصل للإجازة. تُستخدم به /api/app/request/{id} للتفاصيل والخط الزمني والموافقة والرفض والإلغاء.
  final String requestId;
  final EmployeeRefDto employee;
  /// مقدم الطلب الفعلي.
  final UserRefDto submittedBy;
  final bool isOnBehalf;
  final LeaveKind kind;
  final DateTime? date;
  final String? fromTime;
  final String? toTime;
  final DateTime? startDate;
  final DateTime? endDate;
  final OtherLeaveTypeOptionDto? otherLeaveType;
  final String? notes;
  final RequestStatus status;
  final LeaveBalanceEffect balanceEffect;
  /// 0 للزمنية.
  final double workingDays;
  final double? requestedHours;
  final double? hourlyQuotaHoursUsed;
  final double? hourlyExcessHours;
  final double annualDays;
  final double fromCarriedDays;
  final double fromEntitlementDays;
  final List<LeaveYearSegmentDto> segments;
  final List<AttachmentDto> attachments;
  final DateTime creationTime;

  factory LeaveRequestDto.fromJson(Map<String, dynamic> json) => LeaveRequestDto(
        requestId: (json['requestId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        submittedBy: UserRefDto.fromJson((json['submittedBy']) as Map<String, dynamic>),
        isOnBehalf: (json['isOnBehalf']) as bool,
        kind: LeaveKind.fromJson(json['kind']),
        date: json['date'] == null ? null : hrDateFromJson((json['date']) as String),
        fromTime: json['fromTime'] == null ? null : (json['fromTime']) as String,
        toTime: json['toTime'] == null ? null : (json['toTime']) as String,
        startDate: json['startDate'] == null ? null : hrDateFromJson((json['startDate']) as String),
        endDate: json['endDate'] == null ? null : hrDateFromJson((json['endDate']) as String),
        otherLeaveType: json['otherLeaveType'] == null ? null : OtherLeaveTypeOptionDto.fromJson((json['otherLeaveType']) as Map<String, dynamic>),
        notes: json['notes'] == null ? null : (json['notes']) as String,
        status: RequestStatus.fromJson(json['status']),
        balanceEffect: LeaveBalanceEffect.fromJson(json['balanceEffect']),
        workingDays: ((json['workingDays']) as num).toDouble(),
        requestedHours: json['requestedHours'] == null ? null : ((json['requestedHours']) as num).toDouble(),
        hourlyQuotaHoursUsed: json['hourlyQuotaHoursUsed'] == null ? null : ((json['hourlyQuotaHoursUsed']) as num).toDouble(),
        hourlyExcessHours: json['hourlyExcessHours'] == null ? null : ((json['hourlyExcessHours']) as num).toDouble(),
        annualDays: ((json['annualDays']) as num).toDouble(),
        fromCarriedDays: ((json['fromCarriedDays']) as num).toDouble(),
        fromEntitlementDays: ((json['fromEntitlementDays']) as num).toDouble(),
        segments: ((json['segments']) as List).map((e) => LeaveYearSegmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
        attachments: ((json['attachments']) as List).map((e) => AttachmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'requestId': requestId,
        'employee': employee.toJson(),
        'submittedBy': submittedBy.toJson(),
        'isOnBehalf': isOnBehalf,
        'kind': kind.toJson(),
        if (date != null) 'date': hrDateToJson(date!),
        if (fromTime != null) 'fromTime': fromTime!,
        if (toTime != null) 'toTime': toTime!,
        if (startDate != null) 'startDate': hrDateToJson(startDate!),
        if (endDate != null) 'endDate': hrDateToJson(endDate!),
        if (otherLeaveType != null) 'otherLeaveType': otherLeaveType!.toJson(),
        if (notes != null) 'notes': notes!,
        'status': status.toJson(),
        'balanceEffect': balanceEffect.toJson(),
        'workingDays': workingDays,
        if (requestedHours != null) 'requestedHours': requestedHours!,
        if (hourlyQuotaHoursUsed != null) 'hourlyQuotaHoursUsed': hourlyQuotaHoursUsed!,
        if (hourlyExcessHours != null) 'hourlyExcessHours': hourlyExcessHours!,
        'annualDays': annualDays,
        'fromCarriedDays': fromCarriedDays,
        'fromEntitlementDays': fromEntitlementDays,
        'segments': segments.map((e) => e.toJson()).toList(),
        'attachments': attachments.map((e) => e.toJson()).toList(),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class LeaveRequestPagedResultDto {
  const LeaveRequestPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<LeaveRequestDto> items;

  factory LeaveRequestPagedResultDto.fromJson(Map<String, dynamic> json) => LeaveRequestPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => LeaveRequestDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class LeaveAttachmentUploadDto {
  const LeaveAttachmentUploadDto({
    required this.file,
  });

  /// PDF أو JPEG أو PNG، بحد أقصى 10 ميجابايت. يُفحص من الفيروسات.
  final MultipartFile file;

  Map<String, dynamic> toJson() => <String, dynamic>{
      };

  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,
  /// one part per file for arrays (x-server-requirements.multipart).
  FormData toFormData() {
    final form = FormData();
    form.files.add(MapEntry('file', file));
    return form;
  }
}

class LeaveCalendarEntryDto {
  const LeaveCalendarEntryDto({
    required this.requestId,
    required this.employee,
    required this.kind,
    this.otherLeaveTypeName,
    required this.startDate,
    required this.endDate,
    this.fromTime,
    this.toTime,
    required this.status,
    this.workingDays,
    this.requestedHours,
  });

  final String requestId;
  final EmployeeRefDto employee;
  final LeaveKind kind;
  /// Localized. للنوع other فقط.
  final String? otherLeaveTypeName;
  /// للزمنية يساوي تاريخها.
  final DateTime startDate;
  /// للزمنية يساوي تاريخها.
  final DateTime endDate;
  final String? fromTime;
  final String? toTime;
  final RequestStatus status;
  /// null للزمنية.
  final double? workingDays;
  /// للزمنية فقط.
  final double? requestedHours;

  factory LeaveCalendarEntryDto.fromJson(Map<String, dynamic> json) => LeaveCalendarEntryDto(
        requestId: (json['requestId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        kind: LeaveKind.fromJson(json['kind']),
        otherLeaveTypeName: json['otherLeaveTypeName'] == null ? null : (json['otherLeaveTypeName']) as String,
        startDate: hrDateFromJson((json['startDate']) as String),
        endDate: hrDateFromJson((json['endDate']) as String),
        fromTime: json['fromTime'] == null ? null : (json['fromTime']) as String,
        toTime: json['toTime'] == null ? null : (json['toTime']) as String,
        status: RequestStatus.fromJson(json['status']),
        workingDays: json['workingDays'] == null ? null : ((json['workingDays']) as num).toDouble(),
        requestedHours: json['requestedHours'] == null ? null : ((json['requestedHours']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'requestId': requestId,
        'employee': employee.toJson(),
        'kind': kind.toJson(),
        if (otherLeaveTypeName != null) 'otherLeaveTypeName': otherLeaveTypeName!,
        'startDate': hrDateToJson(startDate),
        'endDate': hrDateToJson(endDate),
        if (fromTime != null) 'fromTime': fromTime!,
        if (toTime != null) 'toTime': toTime!,
        'status': status.toJson(),
        if (workingDays != null) 'workingDays': workingDays!,
        if (requestedHours != null) 'requestedHours': requestedHours!,
      };
}

class LeaveCalendarEntryPagedResultDto {
  const LeaveCalendarEntryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  /// Total matching records before paging.
  final int totalCount;
  final List<LeaveCalendarEntryDto> items;

  factory LeaveCalendarEntryPagedResultDto.fromJson(Map<String, dynamic> json) => LeaveCalendarEntryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => LeaveCalendarEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// الحالة الفعلية للطلب في مسار الموافقة. يستخدمها المعتمدون والموارد البشرية، وتشير إليها الوحدات الأخرى. pendingManager: بانتظار الموافقة الأولى. المعتمد مدير القسم المرتبط بالموظف، أو مسؤول النظام إذا كان مقدم الطلب مدير قسم (currentApproverRole يحدد ذلك). pendingHr: بانتظار الموارد البشرية. pendingExecutive: بانتظار المدير التنفيذي. pendingDestination: انتهت الموافقات أو لا تلزم، وبانتظار تنفيذ ا...
enum RequestStatus {
  pendingManager('pendingManager'),
  pendingHr('pendingHr'),
  pendingExecutive('pendingExecutive'),
  pendingDestination('pendingDestination'),
  approved('approved'),
  rejected('rejected'),
  completed('completed'),
  cancelled('cancelled'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const RequestStatus(this.value);
  final String value;

  static RequestStatus fromJson(Object? json) {
    for (final v in RequestStatus.values) {
      if (v.value == json) return v;
    }
    return RequestStatus.$unknown;
  }

  String toJson() => value;
}

/// الحالة كما يراها مقدم الطلب، وفق القسم 4 من الوثيقة. يجب أن تعرضها شاشات «طلباتي» بدل RequestStatus. pendingManager = بانتظار مدير القسم. pendingHr = بانتظار الموارد البشرية. approved = موافق عليه. rejected = مرفوض. completed = مكتمل. التحويل من RequestStatus: pendingDestination تظهر approved، والبقية بالاسم نفسه. قيمتان خارج قائمة الموظف الخمس في الوثيقة: pendingExecutive (بانتظار المدير التنفيذي...
enum RequestEmployeeStatus {
  pendingManager('pendingManager'),
  pendingHr('pendingHr'),
  pendingExecutive('pendingExecutive'),
  approved('approved'),
  rejected('rejected'),
  completed('completed'),
  cancelled('cancelled'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const RequestEmployeeStatus(this.value);
  final String value;

  static RequestEmployeeStatus fromJson(Object? json) {
    for (final v in RequestEmployeeStatus.values) {
      if (v.value == json) return v;
    }
    return RequestEmployeeStatus.$unknown;
  }

  String toJson() => value;
}

/// جهة التنفيذ بعد الموافقات. none: الموافقة الأخيرة تنهي الطلب بالحالة approved (عمود الإجازة وتصحيح البصمة في المصفوفة). store: مسؤول المخزن. accountant: المحاسب.
enum RequestDestination {
  none('none'),
  store('store'),
  accountant('accountant'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const RequestDestination(this.value);
  final String value;

  static RequestDestination fromJson(Object? json) {
    for (final v in RequestDestination.values) {
      if (v.value == json) return v;
    }
    return RequestDestination.$unknown;
  }

  String toJson() => value;
}

/// notReached: لم تصل إليها بعد أو توقف الطلب قبلها. pending: الخطوة الحالية. approved: موافقة، وهي أيضًا إنجاز جهة التنفيذ. approvedBySubmission: عُدت موافقة مدير القسم تمت لأنه قدم الطلب نيابة عن الموظف. rejected: رفض.
enum RequestStepDecision {
  notReached('notReached'),
  pending('pending'),
  approved('approved'),
  approvedBySubmission('approvedBySubmission'),
  rejected('rejected'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const RequestStepDecision(this.value);
  final String value;

  static RequestStepDecision fromJson(Object? json) {
    for (final v in RequestStepDecision.values) {
      if (v.value == json) return v;
    }
    return RequestStepDecision.$unknown;
  }

  String toJson() => value;
}

/// decide: الإجراء عبر approve وreject على الطلب. stockIssue: الإجراء عبر مسار /api/app/stock-issue باستخدام stockIssueId.
enum RequestInboxActionKind {
  decide('decide'),
  stockIssue('stockIssue'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const RequestInboxActionKind(this.value);
  final String value;

  static RequestInboxActionKind fromJson(Object? json) {
    for (final v in RequestInboxActionKind.values) {
      if (v.value == json) return v;
    }
    return RequestInboxActionKind.$unknown;
  }

  String toJson() => value;
}

/// نوع طلب للمستخدم العادي، باسم مترجم.
class RequestTypeDto {
  const RequestTypeDto({
    required this.id,
    required this.code,
    required this.name,
    required this.destination,
    required this.usesDedicatedEndpoint,
    required this.requiresItem,
  });

  /// معرف النوع.
  final String id;
  /// رمز ثابت. الأنواع المبذورة: leave, attendanceCorrection, storeItem, accountant, administrativeNeed.
  final String code;
  /// الاسم حسب Accept-Language.
  final String name;
  final RequestDestination destination;
  /// true للنوعين leave وattendanceCorrection. تقديمهما عبر POST /api/app/leave-request وPOST /api/app/attendance-correction وليس POST /api/app/request.
  final bool usesDedicatedEndpoint;
  /// true عندما destination = store. عندها يلزم itemId وquantity عند التقديم.
  final bool requiresItem;

  factory RequestTypeDto.fromJson(Map<String, dynamic> json) => RequestTypeDto(
        id: (json['id']) as String,
        code: (json['code']) as String,
        name: (json['name']) as String,
        destination: RequestDestination.fromJson(json['destination']),
        usesDedicatedEndpoint: (json['usesDedicatedEndpoint']) as bool,
        requiresItem: (json['requiresItem']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'code': code,
        'name': name,
        'destination': destination.toJson(),
        'usesDedicatedEndpoint': usesDedicatedEndpoint,
        'requiresItem': requiresItem,
      };
}

/// صفحة من RequestTypeDto.
class RequestTypePagedResultDto {
  const RequestTypePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<RequestTypeDto> items;

  factory RequestTypePagedResultDto.fromJson(Map<String, dynamic> json) => RequestTypePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => RequestTypeDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// نوع طلب لمسؤول النظام.
class RequestTypeAdminDto {
  const RequestTypeAdminDto({
    required this.id,
    required this.code,
    required this.nameAr,
    required this.nameEn,
    required this.destination,
    required this.isActive,
    required this.isSystem,
    required this.concurrencyStamp,
    required this.creationTime,
    this.lastModificationTime,
  });

  /// معرف النوع.
  final String id;
  /// رمز ثابت وفريد.
  final String code;
  /// الاسم بالعربية.
  final String nameAr;
  /// الاسم بالإنجليزية.
  final String nameEn;
  final RequestDestination destination;
  /// النوع غير المفعل لا يظهر عند التقديم، وتكمل طلباته القائمة مسارها.
  final bool isActive;
  /// نوع مبذور. لا يتغير رمزه ولا جهته.
  final bool isSystem;
  /// يُعاد إرساله في التحديث. عدم المطابقة يعيد 409.
  final String concurrencyStamp;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;
  /// وقت آخر تعديل UTC.
  final DateTime? lastModificationTime;

  factory RequestTypeAdminDto.fromJson(Map<String, dynamic> json) => RequestTypeAdminDto(
        id: (json['id']) as String,
        code: (json['code']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        destination: RequestDestination.fromJson(json['destination']),
        isActive: (json['isActive']) as bool,
        isSystem: (json['isSystem']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'code': code,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'destination': destination.toJson(),
        'isActive': isActive,
        'isSystem': isSystem,
        'concurrencyStamp': concurrencyStamp,
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
      };
}

/// صفحة من RequestTypeAdminDto.
class RequestTypeAdminPagedResultDto {
  const RequestTypeAdminPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<RequestTypeAdminDto> items;

  factory RequestTypeAdminPagedResultDto.fromJson(Map<String, dynamic> json) => RequestTypeAdminPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => RequestTypeAdminDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// إنشاء نوع طلب.
class RequestTypeCreateInputDto {
  const RequestTypeCreateInputDto({
    required this.code,
    required this.nameAr,
    required this.nameEn,
    required this.destination,
    required this.isActive,
  });

  /// حروف إنجليزية بصيغة camelCase. فريد.
  final String code;
  /// الاسم بالعربية.
  final String nameAr;
  /// الاسم بالإنجليزية.
  final String nameEn;
  final RequestDestination destination;
  /// تفعيل النوع.
  final bool isActive;

  factory RequestTypeCreateInputDto.fromJson(Map<String, dynamic> json) => RequestTypeCreateInputDto(
        code: (json['code']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        destination: RequestDestination.fromJson(json['destination']),
        isActive: (json['isActive']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'code': code,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'destination': destination.toJson(),
        'isActive': isActive,
      };
}

/// تعديل نوع طلب. الرمز لا يتغير.
class RequestTypeUpdateInputDto {
  const RequestTypeUpdateInputDto({
    required this.nameAr,
    required this.nameEn,
    required this.destination,
    required this.isActive,
    required this.concurrencyStamp,
  });

  /// الاسم بالعربية.
  final String nameAr;
  /// الاسم بالإنجليزية.
  final String nameEn;
  /// لا يتغير للأنواع المبذورة.
  final RequestDestination destination;
  /// تفعيل النوع.
  final bool isActive;
  /// آخر قيمة مقروءة.
  final String concurrencyStamp;

  factory RequestTypeUpdateInputDto.fromJson(Map<String, dynamic> json) => RequestTypeUpdateInputDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        destination: RequestDestination.fromJson(json['destination']),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'destination': destination.toJson(),
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// مرجع مختصر لنوع الطلب.
class RequestTypeRefDto {
  const RequestTypeRefDto({
    required this.id,
    required this.code,
    required this.name,
    required this.destination,
  });

  /// معرف النوع.
  final String id;
  /// الرمز الثابت.
  final String code;
  /// الاسم حسب Accept-Language.
  final String name;
  final RequestDestination destination;

  factory RequestTypeRefDto.fromJson(Map<String, dynamic> json) => RequestTypeRefDto(
        id: (json['id']) as String,
        code: (json['code']) as String,
        name: (json['name']) as String,
        destination: RequestDestination.fromJson(json['destination']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'code': code,
        'name': name,
        'destination': destination.toJson(),
      };
}

/// تقديم طلب عام.
class RequestSubmitInputDto {
  const RequestSubmitInputDto({
    required this.requestTypeId,
    this.onBehalfOfEmployeeId,
    required this.description,
    this.itemId,
    this.quantity,
  });

  /// نوع الطلب.
  final String requestTypeId;
  /// موظف من فريق مدير القسم عند التقديم نيابة عنه. null للتقديم الشخصي.
  final String? onBehalfOfEmployeeId;
  /// وصف الطلب.
  final String description;
  /// الصنف. مطلوب عندما destination = store، ويجب أن يكون null في غيره.
  final String? itemId;
  /// الكمية، أكبر من صفر. مطلوبة عندما destination = store، ويجب أن تكون null في غيره.
  final double? quantity;

  factory RequestSubmitInputDto.fromJson(Map<String, dynamic> json) => RequestSubmitInputDto(
        requestTypeId: (json['requestTypeId']) as String,
        onBehalfOfEmployeeId: json['onBehalfOfEmployeeId'] == null ? null : (json['onBehalfOfEmployeeId']) as String,
        description: (json['description']) as String,
        itemId: json['itemId'] == null ? null : (json['itemId']) as String,
        quantity: json['quantity'] == null ? null : ((json['quantity']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'requestTypeId': requestTypeId,
        if (onBehalfOfEmployeeId != null) 'onBehalfOfEmployeeId': onBehalfOfEmployeeId!,
        'description': description,
        if (itemId != null) 'itemId': itemId!,
        if (quantity != null) 'quantity': quantity!,
      };
}

/// طلب في قائمة «طلباتي». لا يحمل RequestStatus عمدًا.
class MyRequestDto {
  const MyRequestDto({
    required this.id,
    required this.type,
    required this.detailsSummary,
    required this.employeeStatus,
    required this.statusLabel,
    this.currentStageRole,
    this.stockIssueStatusLabel,
    this.rejectionReason,
    required this.isOnBehalf,
    required this.submittedBy,
    required this.creationTime,
    this.lastModificationTime,
  });

  /// معرف الطلب.
  final String id;
  final RequestTypeRefDto type;
  /// ملخص مترجم يولده الخادم، مثل نوع الإجازة وتواريخها أو الصنف وكميته.
  final String detailsSummary;
  final RequestEmployeeStatus employeeStatus;
  /// نص الحالة حسب Accept-Language. عند pendingManager والمعتمد مسؤول النظام يكون «بانتظار مسؤول النظام».
  final String statusLabel;
  /// أين وصل الطلب الآن. storeOfficer أو accountant أثناء التنفيذ رغم أن employeeStatus = approved. null في الحالات النهائية.
  final AppRole? currentStageRole;
  /// حالة الصرف المترجمة، مثل «بانتظار الشراء». null لغير طلبات المخزن أو قبل إنشاء الصرف.
  final String? stockIssueStatusLabel;
  /// سبب الرفض. يظهر للموظف عند employeeStatus = rejected.
  final String? rejectionReason;
  /// true إذا قدمه مدير القسم نيابة عن الموظف.
  final bool isOnBehalf;
  /// المقدم الفعلي.
  final UserRefDto submittedBy;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;
  /// وقت آخر تعديل UTC.
  final DateTime? lastModificationTime;

  factory MyRequestDto.fromJson(Map<String, dynamic> json) => MyRequestDto(
        id: (json['id']) as String,
        type: RequestTypeRefDto.fromJson((json['type']) as Map<String, dynamic>),
        detailsSummary: (json['detailsSummary']) as String,
        employeeStatus: RequestEmployeeStatus.fromJson(json['employeeStatus']),
        statusLabel: (json['statusLabel']) as String,
        currentStageRole: json['currentStageRole'] == null ? null : AppRole.fromJson(json['currentStageRole']),
        stockIssueStatusLabel: json['stockIssueStatusLabel'] == null ? null : (json['stockIssueStatusLabel']) as String,
        rejectionReason: json['rejectionReason'] == null ? null : (json['rejectionReason']) as String,
        isOnBehalf: (json['isOnBehalf']) as bool,
        submittedBy: UserRefDto.fromJson((json['submittedBy']) as Map<String, dynamic>),
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'type': type.toJson(),
        'detailsSummary': detailsSummary,
        'employeeStatus': employeeStatus.toJson(),
        'statusLabel': statusLabel,
        if (currentStageRole != null) 'currentStageRole': currentStageRole!.toJson(),
        if (stockIssueStatusLabel != null) 'stockIssueStatusLabel': stockIssueStatusLabel!,
        if (rejectionReason != null) 'rejectionReason': rejectionReason!,
        'isOnBehalf': isOnBehalf,
        'submittedBy': submittedBy.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
      };
}

/// صفحة من MyRequestDto.
class MyRequestPagedResultDto {
  const MyRequestPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<MyRequestDto> items;

  factory MyRequestPagedResultDto.fromJson(Map<String, dynamic> json) => MyRequestPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => MyRequestDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// عنصر في صندوق «بانتظار إجرائي». لا يحوي مهلة موافقة لأنه لا توجد مهلة.
class RequestInboxItemDto {
  const RequestInboxItemDto({
    required this.id,
    required this.type,
    required this.employee,
    required this.submittedBy,
    required this.isOnBehalf,
    this.detailsSummary,
    required this.status,
    required this.currentApproverRole,
    required this.actionKind,
    this.stockIssueId,
    this.delegatedFrom,
    required this.waitingSince,
    required this.creationTime,
  });

  /// معرف الطلب.
  final String id;
  final RequestTypeRefDto type;
  /// صاحب الطلب.
  final EmployeeRefDto employee;
  /// المقدم الفعلي.
  final UserRefDto submittedBy;
  /// true إذا قُدم نيابة عن الموظف.
  final bool isOnBehalf;
  /// ملخص مترجم يولده الخادم.
  final String? detailsSummary;
  final RequestStatus status;
  final AppRole currentApproverRole;
  final RequestInboxActionKind actionKind;
  /// معرف الصرف عندما actionKind = stockIssue.
  final String? stockIssueId;
  /// صاحب الصلاحية الأصلي إذا وصل العنصر عبر تفويض ساري.
  final UserRefDto? delegatedFrom;
  /// وقت وصول الطلب إلى الخطوة الحالية. للعرض فقط، ولا يترتب عليه إجراء تلقائي.
  final DateTime waitingSince;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;

  factory RequestInboxItemDto.fromJson(Map<String, dynamic> json) => RequestInboxItemDto(
        id: (json['id']) as String,
        type: RequestTypeRefDto.fromJson((json['type']) as Map<String, dynamic>),
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        submittedBy: UserRefDto.fromJson((json['submittedBy']) as Map<String, dynamic>),
        isOnBehalf: (json['isOnBehalf']) as bool,
        detailsSummary: json['detailsSummary'] == null ? null : (json['detailsSummary']) as String,
        status: RequestStatus.fromJson(json['status']),
        currentApproverRole: AppRole.fromJson(json['currentApproverRole']),
        actionKind: RequestInboxActionKind.fromJson(json['actionKind']),
        stockIssueId: json['stockIssueId'] == null ? null : (json['stockIssueId']) as String,
        delegatedFrom: json['delegatedFrom'] == null ? null : UserRefDto.fromJson((json['delegatedFrom']) as Map<String, dynamic>),
        waitingSince: DateTime.parse((json['waitingSince']) as String),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'type': type.toJson(),
        'employee': employee.toJson(),
        'submittedBy': submittedBy.toJson(),
        'isOnBehalf': isOnBehalf,
        if (detailsSummary != null) 'detailsSummary': detailsSummary!,
        'status': status.toJson(),
        'currentApproverRole': currentApproverRole.toJson(),
        'actionKind': actionKind.toJson(),
        if (stockIssueId != null) 'stockIssueId': stockIssueId!,
        if (delegatedFrom != null) 'delegatedFrom': delegatedFrom!.toJson(),
        'waitingSince': hrDateTimeToJson(waitingSince),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

/// صفحة من RequestInboxItemDto.
class RequestInboxItemPagedResultDto {
  const RequestInboxItemPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<RequestInboxItemDto> items;

  factory RequestInboxItemPagedResultDto.fromJson(Map<String, dynamic> json) => RequestInboxItemPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => RequestInboxItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// طلب في قائمة الموارد البشرية أو طلبات الفريق.
class RequestSummaryDto {
  const RequestSummaryDto({
    required this.id,
    required this.type,
    required this.employee,
    required this.submittedBy,
    required this.isOnBehalf,
    this.detailsSummary,
    required this.status,
    this.currentApproverRole,
    required this.creationTime,
    this.lastModificationTime,
  });

  /// معرف الطلب.
  final String id;
  final RequestTypeRefDto type;
  /// صاحب الطلب.
  final EmployeeRefDto employee;
  /// المقدم الفعلي.
  final UserRefDto submittedBy;
  /// true إذا قُدم نيابة عن الموظف.
  final bool isOnBehalf;
  /// ملخص مترجم يولده الخادم.
  final String? detailsSummary;
  final RequestStatus status;
  /// null في الحالات النهائية.
  final AppRole? currentApproverRole;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;
  /// وقت آخر تعديل UTC.
  final DateTime? lastModificationTime;

  factory RequestSummaryDto.fromJson(Map<String, dynamic> json) => RequestSummaryDto(
        id: (json['id']) as String,
        type: RequestTypeRefDto.fromJson((json['type']) as Map<String, dynamic>),
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        submittedBy: UserRefDto.fromJson((json['submittedBy']) as Map<String, dynamic>),
        isOnBehalf: (json['isOnBehalf']) as bool,
        detailsSummary: json['detailsSummary'] == null ? null : (json['detailsSummary']) as String,
        status: RequestStatus.fromJson(json['status']),
        currentApproverRole: json['currentApproverRole'] == null ? null : AppRole.fromJson(json['currentApproverRole']),
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'type': type.toJson(),
        'employee': employee.toJson(),
        'submittedBy': submittedBy.toJson(),
        'isOnBehalf': isOnBehalf,
        if (detailsSummary != null) 'detailsSummary': detailsSummary!,
        'status': status.toJson(),
        if (currentApproverRole != null) 'currentApproverRole': currentApproverRole!.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
      };
}

/// صفحة من RequestSummaryDto.
class RequestSummaryPagedResultDto {
  const RequestSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<RequestSummaryDto> items;

  factory RequestSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => RequestSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => RequestSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// خطوة في مسار الطلب.
class RequestStepDto {
  const RequestStepDto({
    required this.order,
    required this.approverRole,
    this.approver,
    required this.decision,
    this.decidedBy,
    this.delegatedFrom,
    this.decidedAt,
    this.note,
  });

  /// ترتيب الخطوة، يبدأ من 1.
  final int order;
  /// الدور المطلوب. departmentManager للمدير المرتبط، وsystemAdmin لمديري الأقسام، وstoreOfficer أو accountant لخطوة التنفيذ.
  final AppRole approverRole;
  /// المعتمد المسمى إن كان محددًا، مثل مدير القسم. null إذا كانت الخطوة لأي صاحب دور.
  final UserRefDto? approver;
  final RequestStepDecision decision;
  /// من اتخذ القرار فعليًا.
  final UserRefDto? decidedBy;
  /// صاحب الصلاحية الأصلي إذا كان القرار بالتفويض.
  final UserRefDto? delegatedFrom;
  /// وقت القرار.
  final DateTime? decidedAt;
  /// ملاحظة الموافقة أو سبب الرفض.
  final String? note;

  factory RequestStepDto.fromJson(Map<String, dynamic> json) => RequestStepDto(
        order: ((json['order']) as num).toInt(),
        approverRole: AppRole.fromJson(json['approverRole']),
        approver: json['approver'] == null ? null : UserRefDto.fromJson((json['approver']) as Map<String, dynamic>),
        decision: RequestStepDecision.fromJson(json['decision']),
        decidedBy: json['decidedBy'] == null ? null : UserRefDto.fromJson((json['decidedBy']) as Map<String, dynamic>),
        delegatedFrom: json['delegatedFrom'] == null ? null : UserRefDto.fromJson((json['delegatedFrom']) as Map<String, dynamic>),
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse((json['decidedAt']) as String),
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'order': order,
        'approverRole': approverRole.toJson(),
        if (approver != null) 'approver': approver!.toJson(),
        'decision': decision.toJson(),
        if (decidedBy != null) 'decidedBy': decidedBy!.toJson(),
        if (delegatedFrom != null) 'delegatedFrom': delegatedFrom!.toJson(),
        if (decidedAt != null) 'decidedAt': hrDateTimeToJson(decidedAt!),
        if (note != null) 'note': note!,
      };
}

/// تفاصيل الطلب مع الخط الزمني. كل بيانات الموظف عبر EmployeeRefDto، ولا توجد حقول مالية.
class RequestDetailsDto {
  const RequestDetailsDto({
    required this.id,
    required this.type,
    required this.employee,
    required this.submittedBy,
    required this.isOnBehalf,
    this.description,
    this.detailsSummary,
    required this.status,
    required this.employeeStatus,
    required this.statusLabel,
    this.currentApproverRole,
    this.rejectionReason,
    required this.steps,
    required this.timeline,
    this.stockIssue,
    this.sourceMemoId,
    required this.canApprove,
    required this.canReject,
    required this.canCancel,
    required this.creationTime,
    this.lastModificationTime,
  });

  /// معرف الطلب.
  final String id;
  final RequestTypeRefDto type;
  /// صاحب الطلب.
  final EmployeeRefDto employee;
  /// المقدم الفعلي، ويختلف عن صاحب الطلب عند التقديم نيابة عنه.
  final UserRefDto submittedBy;
  /// true إذا قُدم نيابة عن الموظف.
  final bool isOnBehalf;
  /// وصف الطلب. null للإجازة وتصحيح البصمة. ملخصهما في detailsSummary، وتفاصيلهما في GET /api/app/leave-request/{id} وGET /api/app/attendance-correction/{id} بمعرف الطلب نفسه.
  final String? description;
  /// ملخص مترجم يولده الخادم.
  final String? detailsSummary;
  /// الحالة الفعلية. شاشات المعتمدين تعرضها، وشاشات مقدم الطلب تعرض employeeStatus.
  final RequestStatus status;
  final RequestEmployeeStatus employeeStatus;
  /// نص الحالة المناسب للمستخدم الحالي حسب Accept-Language.
  final String statusLabel;
  /// null في الحالات النهائية.
  final AppRole? currentApproverRole;
  /// سبب الرفض عند status = rejected.
  final String? rejectionReason;
  /// خطوات المسار المبنية عند التقديم حسب مقدم الطلب وجهته.
  final List<RequestStepDto> steps;
  /// من قرر ومتى ولماذا، مرتب تصاعديًا. قيم action: submitted, submittedOnBehalf, approved, rejected, cancelled, approverReassigned, stockIssueAcknowledged, stockIssuePurchasing, stockIssueDelivered, completed.
  final List<TimelineEntryDto> timeline;
  /// حالة الصرف لطلبات المخزن بعد إنشائه.
  final StockIssueStatusDto? stockIssue;
  /// المطالعة التي تحول منها الطلب.
  final String? sourceMemoId;
  /// المستخدم الحالي يستطيع استدعاء approve الآن.
  final bool canApprove;
  /// المستخدم الحالي يستطيع استدعاء reject الآن.
  final bool canReject;
  /// المستخدم الحالي يستطيع استدعاء cancel الآن.
  final bool canCancel;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;
  /// وقت آخر تعديل UTC.
  final DateTime? lastModificationTime;

  factory RequestDetailsDto.fromJson(Map<String, dynamic> json) => RequestDetailsDto(
        id: (json['id']) as String,
        type: RequestTypeRefDto.fromJson((json['type']) as Map<String, dynamic>),
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        submittedBy: UserRefDto.fromJson((json['submittedBy']) as Map<String, dynamic>),
        isOnBehalf: (json['isOnBehalf']) as bool,
        description: json['description'] == null ? null : (json['description']) as String,
        detailsSummary: json['detailsSummary'] == null ? null : (json['detailsSummary']) as String,
        status: RequestStatus.fromJson(json['status']),
        employeeStatus: RequestEmployeeStatus.fromJson(json['employeeStatus']),
        statusLabel: (json['statusLabel']) as String,
        currentApproverRole: json['currentApproverRole'] == null ? null : AppRole.fromJson(json['currentApproverRole']),
        rejectionReason: json['rejectionReason'] == null ? null : (json['rejectionReason']) as String,
        steps: ((json['steps']) as List).map((e) => RequestStepDto.fromJson((e) as Map<String, dynamic>)).toList(),
        timeline: ((json['timeline']) as List).map((e) => TimelineEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
        stockIssue: json['stockIssue'] == null ? null : StockIssueStatusDto.fromJson((json['stockIssue']) as Map<String, dynamic>),
        sourceMemoId: json['sourceMemoId'] == null ? null : (json['sourceMemoId']) as String,
        canApprove: (json['canApprove']) as bool,
        canReject: (json['canReject']) as bool,
        canCancel: (json['canCancel']) as bool,
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'type': type.toJson(),
        'employee': employee.toJson(),
        'submittedBy': submittedBy.toJson(),
        'isOnBehalf': isOnBehalf,
        if (description != null) 'description': description!,
        if (detailsSummary != null) 'detailsSummary': detailsSummary!,
        'status': status.toJson(),
        'employeeStatus': employeeStatus.toJson(),
        'statusLabel': statusLabel,
        if (currentApproverRole != null) 'currentApproverRole': currentApproverRole!.toJson(),
        if (rejectionReason != null) 'rejectionReason': rejectionReason!,
        'steps': steps.map((e) => e.toJson()).toList(),
        'timeline': timeline.map((e) => e.toJson()).toList(),
        if (stockIssue != null) 'stockIssue': stockIssue!.toJson(),
        if (sourceMemoId != null) 'sourceMemoId': sourceMemoId!,
        'canApprove': canApprove,
        'canReject': canReject,
        'canCancel': canCancel,
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
      };
}

/// leave: مطالعة إجازة، وتتحول إلى طلب إجازة. problem: مطالعة مشكلة، وتتحول إلى بلاغ.
enum MemoKind {
  leave('leave'),
  problem('problem'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const MemoKind(this.value);
  final String value;

  static MemoKind fromJson(Object? json) {
    for (final v in MemoKind.values) {
      if (v.value == json) return v;
    }
    return MemoKind.$unknown;
  }

  String toJson() => value;
}

/// pendingManager: عند المدير. pendingHr: عند الموارد البشرية. convertedToRequest: تحولت إلى طلب إجازة. convertedToIncident: تحولت إلى بلاغ. closed: أُغلقت دون تحويل مع سبب. pendingExecutive: عند المدير التنفيذي.
enum MemoStatus {
  pendingManager('pendingManager'),
  pendingHr('pendingHr'),
  convertedToRequest('convertedToRequest'),
  convertedToIncident('convertedToIncident'),
  closed('closed'),
  pendingExecutive('pendingExecutive'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const MemoStatus(this.value);
  final String value;

  static MemoStatus fromJson(Object? json) {
    for (final v in MemoStatus.values) {
      if (v.value == json) return v;
    }
    return MemoStatus.$unknown;
  }

  String toJson() => value;
}

/// leaveRequest لمطالعة الإجازة، وincident لمطالعة المشكلة.
enum MemoConvertTarget {
  leaveRequest('leaveRequest'),
  incident('incident'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const MemoConvertTarget(this.value);
  final String value;

  static MemoConvertTarget fromJson(Object? json) {
    for (final v in MemoConvertTarget.values) {
      if (v.value == json) return v;
    }
    return MemoConvertTarget.$unknown;
  }

  String toJson() => value;
}

/// رفع مطالعة.
class MemoCreateInputDto {
  const MemoCreateInputDto({
    required this.kind,
    required this.content,
  });

  final MemoKind kind;
  /// نص المطالعة.
  final String content;

  factory MemoCreateInputDto.fromJson(Map<String, dynamic> json) => MemoCreateInputDto(
        kind: MemoKind.fromJson(json['kind']),
        content: (json['content']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'kind': kind.toJson(),
        'content': content,
      };
}

/// بيانات الإجازة عند التحويل. hourly: leaveDate وfromTime وtoTime. annual: startDate وendDate. other: otherLeaveTypeId وstartDate وendDate.
class MemoLeaveConversionDto {
  const MemoLeaveConversionDto({
    required this.kind,
    this.otherLeaveTypeId,
    this.startDate,
    this.endDate,
    this.leaveDate,
    this.fromTime,
    this.toTime,
  });

  final LeaveKind kind;
  /// مطلوب عندما kind = other.
  final String? otherLeaveTypeId;
  /// من تاريخ.
  final DateTime? startDate;
  /// إلى تاريخ.
  final DateTime? endDate;
  /// تاريخ الزمنية.
  final DateTime? leaveDate;
  /// من ساعة.
  final String? fromTime;
  /// إلى ساعة.
  final String? toTime;

  factory MemoLeaveConversionDto.fromJson(Map<String, dynamic> json) => MemoLeaveConversionDto(
        kind: LeaveKind.fromJson(json['kind']),
        otherLeaveTypeId: json['otherLeaveTypeId'] == null ? null : (json['otherLeaveTypeId']) as String,
        startDate: json['startDate'] == null ? null : hrDateFromJson((json['startDate']) as String),
        endDate: json['endDate'] == null ? null : hrDateFromJson((json['endDate']) as String),
        leaveDate: json['leaveDate'] == null ? null : hrDateFromJson((json['leaveDate']) as String),
        fromTime: json['fromTime'] == null ? null : (json['fromTime']) as String,
        toTime: json['toTime'] == null ? null : (json['toTime']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'kind': kind.toJson(),
        if (otherLeaveTypeId != null) 'otherLeaveTypeId': otherLeaveTypeId!,
        if (startDate != null) 'startDate': hrDateToJson(startDate!),
        if (endDate != null) 'endDate': hrDateToJson(endDate!),
        if (leaveDate != null) 'leaveDate': hrDateToJson(leaveDate!),
        if (fromTime != null) 'fromTime': fromTime!,
        if (toTime != null) 'toTime': toTime!,
      };
}

/// تحويل مطالعة.
class MemoConvertInputDto {
  const MemoConvertInputDto({
    required this.target,
    this.leave,
    this.itemId,
    this.note,
  });

  final MemoConvertTarget target;
  /// مطلوب عندما target = leaveRequest، ويجب أن يكون null في غيره.
  final MemoLeaveConversionDto? leave;
  /// For target = storeItem only. With target = leaveRequest it must be empty, otherwise Memo:LeaveDetailsInvalid.
  final String? itemId;
  /// ملاحظة تظهر في الخط الزمني.
  final String? note;

  factory MemoConvertInputDto.fromJson(Map<String, dynamic> json) => MemoConvertInputDto(
        target: MemoConvertTarget.fromJson(json['target']),
        leave: json['leave'] == null ? null : MemoLeaveConversionDto.fromJson((json['leave']) as Map<String, dynamic>),
        itemId: json['itemId'] == null ? null : (json['itemId']) as String,
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'target': target.toJson(),
        if (leave != null) 'leave': leave!.toJson(),
        if (itemId != null) 'itemId': itemId!,
        if (note != null) 'note': note!,
      };
}

/// مطالعة في قائمة.
class MemoListItemDto {
  const MemoListItemDto({
    required this.id,
    required this.employee,
    required this.kind,
    required this.contentPreview,
    required this.status,
    required this.statusLabel,
    required this.creationTime,
  });

  /// معرف المطالعة.
  final String id;
  /// كاتب المطالعة.
  final EmployeeRefDto employee;
  final MemoKind kind;
  /// أول 200 حرف من النص.
  final String contentPreview;
  final MemoStatus status;
  /// نص الحالة حسب Accept-Language.
  final String statusLabel;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;

  factory MemoListItemDto.fromJson(Map<String, dynamic> json) => MemoListItemDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        kind: MemoKind.fromJson(json['kind']),
        contentPreview: (json['contentPreview']) as String,
        status: MemoStatus.fromJson(json['status']),
        statusLabel: (json['statusLabel']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'kind': kind.toJson(),
        'contentPreview': contentPreview,
        'status': status.toJson(),
        'statusLabel': statusLabel,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

/// صفحة من MemoListItemDto.
class MemoListItemPagedResultDto {
  const MemoListItemPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<MemoListItemDto> items;

  factory MemoListItemPagedResultDto.fromJson(Map<String, dynamic> json) => MemoListItemPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => MemoListItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// تفاصيل مطالعة مع مسارها.
class MemoDto {
  const MemoDto({
    required this.id,
    required this.employee,
    required this.kind,
    required this.content,
    required this.status,
    required this.statusLabel,
    this.currentHandlerRole,
    this.closeReason,
    this.convertedRequestId,
    this.convertedIncidentId,
    required this.timeline,
    required this.creationTime,
    this.lastModificationTime,
  });

  /// معرف المطالعة.
  final String id;
  /// كاتب المطالعة.
  final EmployeeRefDto employee;
  final MemoKind kind;
  /// نص المطالعة.
  final String content;
  final MemoStatus status;
  /// نص الحالة حسب Accept-Language.
  final String statusLabel;
  /// من عنده المطالعة الآن. null بعد التحويل أو الإغلاق.
  final AppRole? currentHandlerRole;
  /// سبب الإغلاق عند status = closed.
  final String? closeReason;
  /// طلب الإجازة الناتج.
  final String? convertedRequestId;
  /// البلاغ الناتج.
  final String? convertedIncidentId;
  /// قيم action: submitted, forwardedToHr, convertedToRequest, convertedToIncident, closed.
  final List<TimelineEntryDto> timeline;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;
  /// وقت آخر تعديل UTC.
  final DateTime? lastModificationTime;

  factory MemoDto.fromJson(Map<String, dynamic> json) => MemoDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        kind: MemoKind.fromJson(json['kind']),
        content: (json['content']) as String,
        status: MemoStatus.fromJson(json['status']),
        statusLabel: (json['statusLabel']) as String,
        currentHandlerRole: json['currentHandlerRole'] == null ? null : AppRole.fromJson(json['currentHandlerRole']),
        closeReason: json['closeReason'] == null ? null : (json['closeReason']) as String,
        convertedRequestId: json['convertedRequestId'] == null ? null : (json['convertedRequestId']) as String,
        convertedIncidentId: json['convertedIncidentId'] == null ? null : (json['convertedIncidentId']) as String,
        timeline: ((json['timeline']) as List).map((e) => TimelineEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'kind': kind.toJson(),
        'content': content,
        'status': status.toJson(),
        'statusLabel': statusLabel,
        if (currentHandlerRole != null) 'currentHandlerRole': currentHandlerRole!.toJson(),
        if (closeReason != null) 'closeReason': closeReason!,
        if (convertedRequestId != null) 'convertedRequestId': convertedRequestId!,
        if (convertedIncidentId != null) 'convertedIncidentId': convertedIncidentId!,
        'timeline': timeline.map((e) => e.toJson()).toList(),
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
      };
}

/// pendingManager: عند مدير القسم. pendingHr: عند الموارد البشرية. inProgress: قيد المعالجة. resolved: تمت المعالجة، وهي حالة نهائية.
enum IncidentStatus {
  pendingManager('pendingManager'),
  pendingHr('pendingHr'),
  inProgress('inProgress'),
  resolved('resolved'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const IncidentStatus(this.value);
  final String value;

  static IncidentStatus fromJson(Object? json) {
    for (final v in IncidentStatus.values) {
      if (v.value == json) return v;
    }
    return IncidentStatus.$unknown;
  }

  String toJson() => value;
}

/// رفع بلاغ عطل.
class IncidentCreateInputDto {
  const IncidentCreateInputDto({
    this.itemId,
    required this.description,
  });

  /// الصنف المتعطل إن كان من أصناف المخزن، من GET /api/app/item/lookup.
  final String? itemId;
  /// وصف العطل.
  final String description;

  factory IncidentCreateInputDto.fromJson(Map<String, dynamic> json) => IncidentCreateInputDto(
        itemId: json['itemId'] == null ? null : (json['itemId']) as String,
        description: (json['description']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (itemId != null) 'itemId': itemId!,
        'description': description,
      };
}

/// تحديث حالة البلاغ. الانتقالات: pendingHr إلى inProgress أو resolved، وinProgress إلى resolved.
class IncidentStatusUpdateInputDto {
  const IncidentStatusUpdateInputDto({
    required this.status,
    this.note,
    required this.concurrencyStamp,
  });

  final IncidentStatusTarget status;
  /// ملاحظة تظهر للموظف في الخط الزمني.
  final String? note;
  /// آخر قيمة مقروءة.
  final String concurrencyStamp;

  factory IncidentStatusUpdateInputDto.fromJson(Map<String, dynamic> json) => IncidentStatusUpdateInputDto(
        status: IncidentStatusTarget.fromJson(json['status']),
        note: json['note'] == null ? null : (json['note']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'status': status.toJson(),
        if (note != null) 'note': note!,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// بلاغ في قائمة.
class IncidentListItemDto {
  const IncidentListItemDto({
    required this.id,
    required this.employee,
    this.item,
    required this.descriptionPreview,
    required this.status,
    required this.statusLabel,
    required this.creationTime,
  });

  /// معرف البلاغ.
  final String id;
  /// مقدم البلاغ.
  final EmployeeRefDto employee;
  /// الصنف المتعطل.
  final ItemRefDto? item;
  /// أول 200 حرف من الوصف.
  final String descriptionPreview;
  final IncidentStatus status;
  /// نص الحالة حسب Accept-Language.
  final String statusLabel;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;

  factory IncidentListItemDto.fromJson(Map<String, dynamic> json) => IncidentListItemDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        item: json['item'] == null ? null : ItemRefDto.fromJson((json['item']) as Map<String, dynamic>),
        descriptionPreview: (json['descriptionPreview']) as String,
        status: IncidentStatus.fromJson(json['status']),
        statusLabel: (json['statusLabel']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        if (item != null) 'item': item!.toJson(),
        'descriptionPreview': descriptionPreview,
        'status': status.toJson(),
        'statusLabel': statusLabel,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

/// صفحة من IncidentListItemDto.
class IncidentListItemPagedResultDto {
  const IncidentListItemPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<IncidentListItemDto> items;

  factory IncidentListItemPagedResultDto.fromJson(Map<String, dynamic> json) => IncidentListItemPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => IncidentListItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// تفاصيل بلاغ مع مساره، ليرى الموظف أين وصل.
class IncidentDto {
  const IncidentDto({
    required this.id,
    required this.employee,
    this.item,
    required this.description,
    required this.status,
    required this.statusLabel,
    this.currentHandlerRole,
    this.sourceMemoId,
    required this.timeline,
    required this.concurrencyStamp,
    required this.creationTime,
    this.lastModificationTime,
  });

  /// معرف البلاغ.
  final String id;
  /// مقدم البلاغ.
  final EmployeeRefDto employee;
  /// الصنف المتعطل.
  final ItemRefDto? item;
  /// وصف العطل.
  final String description;
  final IncidentStatus status;
  /// نص الحالة حسب Accept-Language.
  final String statusLabel;
  /// من عنده البلاغ الآن. null عند resolved.
  final AppRole? currentHandlerRole;
  /// المطالعة التي تحول منها البلاغ.
  final String? sourceMemoId;
  /// قيم action: submitted, forwardedToHr, inProgress, resolved.
  final List<TimelineEntryDto> timeline;
  /// يُعاد إرساله في التحديث. عدم المطابقة يعيد 409.
  final String concurrencyStamp;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;
  /// وقت آخر تعديل UTC.
  final DateTime? lastModificationTime;

  factory IncidentDto.fromJson(Map<String, dynamic> json) => IncidentDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        item: json['item'] == null ? null : ItemRefDto.fromJson((json['item']) as Map<String, dynamic>),
        description: (json['description']) as String,
        status: IncidentStatus.fromJson(json['status']),
        statusLabel: (json['statusLabel']) as String,
        currentHandlerRole: json['currentHandlerRole'] == null ? null : AppRole.fromJson(json['currentHandlerRole']),
        sourceMemoId: json['sourceMemoId'] == null ? null : (json['sourceMemoId']) as String,
        timeline: ((json['timeline']) as List).map((e) => TimelineEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        if (item != null) 'item': item!.toJson(),
        'description': description,
        'status': status.toJson(),
        'statusLabel': statusLabel,
        if (currentHandlerRole != null) 'currentHandlerRole': currentHandlerRole!.toJson(),
        if (sourceMemoId != null) 'sourceMemoId': sourceMemoId!,
        'timeline': timeline.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
      };
}

/// returnable: تُسترد وتصبح عهدة عند التسليم. consumable: مستهلكة ولا تصبح عهدة.
enum ItemKind {
  returnable('returnable'),
  consumable('consumable'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ItemKind(this.value);
  final String value;

  static ItemKind fromJson(Object? json) {
    for (final v in ItemKind.values) {
      if (v.value == json) return v;
    }
    return ItemKind.$unknown;
  }

  String toJson() => value;
}

/// مرجع صنف للمستخدمين العاديين، دون كميات.
class ItemRefDto {
  const ItemRefDto({
    required this.id,
    required this.name,
    required this.kind,
  });

  /// معرف الصنف.
  final String id;
  /// الاسم حسب Accept-Language.
  final String name;
  final ItemKind kind;

  factory ItemRefDto.fromJson(Map<String, dynamic> json) => ItemRefDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        kind: ItemKind.fromJson(json['kind']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'kind': kind.toJson(),
      };
}

/// صفحة من ItemRefDto.
class ItemRefPagedResultDto {
  const ItemRefPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ItemRefDto> items;

  factory ItemRefPagedResultDto.fromJson(Map<String, dynamic> json) => ItemRefPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ItemRefDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// صنف لمسؤول المخزن.
class ItemDto {
  const ItemDto({
    required this.id,
    required this.nameAr,
    required this.nameEn,
    required this.kind,
    required this.minQuantity,
    required this.quantityOnHand,
    required this.isBelowMinimum,
    required this.isActive,
    required this.concurrencyStamp,
    required this.creationTime,
    this.lastModificationTime,
  });

  /// معرف الصنف.
  final String id;
  /// الاسم بالعربية.
  final String nameAr;
  /// الاسم بالإنجليزية.
  final String nameEn;
  final ItemKind kind;
  /// الحد الأدنى.
  final double minQuantity;
  /// الرصيد الحالي المحسوب من الحركات.
  final double quantityOnHand;
  /// quantityOnHand أقل من minQuantity.
  final bool isBelowMinimum;
  /// غير المفعل لا يظهر عند تقديم الطلبات.
  final bool isActive;
  /// يُعاد إرساله في التحديث. عدم المطابقة يعيد 409.
  final String concurrencyStamp;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;
  /// وقت آخر تعديل UTC.
  final DateTime? lastModificationTime;

  factory ItemDto.fromJson(Map<String, dynamic> json) => ItemDto(
        id: (json['id']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        kind: ItemKind.fromJson(json['kind']),
        minQuantity: ((json['minQuantity']) as num).toDouble(),
        quantityOnHand: ((json['quantityOnHand']) as num).toDouble(),
        isBelowMinimum: (json['isBelowMinimum']) as bool,
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'kind': kind.toJson(),
        'minQuantity': minQuantity,
        'quantityOnHand': quantityOnHand,
        'isBelowMinimum': isBelowMinimum,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
        'creationTime': hrDateTimeToJson(creationTime),
        if (lastModificationTime != null) 'lastModificationTime': hrDateTimeToJson(lastModificationTime!),
      };
}

/// صفحة من ItemDto.
class ItemPagedResultDto {
  const ItemPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ItemDto> items;

  factory ItemPagedResultDto.fromJson(Map<String, dynamic> json) => ItemPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// إضافة صنف. يبدأ رصيده صفرًا، ويزيد عبر استلام المواد.
class ItemCreateInputDto {
  const ItemCreateInputDto({
    required this.nameAr,
    required this.nameEn,
    required this.kind,
    required this.minQuantity,
  });

  /// الاسم بالعربية.
  final String nameAr;
  /// الاسم بالإنجليزية.
  final String nameEn;
  final ItemKind kind;
  /// الحد الأدنى.
  final double minQuantity;

  factory ItemCreateInputDto.fromJson(Map<String, dynamic> json) => ItemCreateInputDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        kind: ItemKind.fromJson(json['kind']),
        minQuantity: ((json['minQuantity']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'kind': kind.toJson(),
        'minQuantity': minQuantity,
      };
}

/// تعديل صنف. الرصيد لا يُعدل هنا.
class ItemUpdateInputDto {
  const ItemUpdateInputDto({
    required this.nameAr,
    required this.nameEn,
    required this.kind,
    required this.minQuantity,
    required this.isActive,
    required this.concurrencyStamp,
  });

  /// الاسم بالعربية.
  final String nameAr;
  /// الاسم بالإنجليزية.
  final String nameEn;
  /// لا يتغير بعد أول حركة.
  final ItemKind kind;
  /// الحد الأدنى.
  final double minQuantity;
  /// تفعيل الصنف.
  final bool isActive;
  /// آخر قيمة مقروءة.
  final String concurrencyStamp;

  factory ItemUpdateInputDto.fromJson(Map<String, dynamic> json) => ItemUpdateInputDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        kind: ItemKind.fromJson(json['kind']),
        minQuantity: ((json['minQuantity']) as num).toDouble(),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'kind': kind.toJson(),
        'minQuantity': minQuantity,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// receive: دخول مواد. issue: تسليم أو عهدة مخصومة من المخزن. return: إرجاع عهدة إلى المخزن. adjustment: تصحيح بحركة عكسية.
enum StockMovementType {
  receive('receive'),
  issue('issue'),
  return_('return'),
  adjustment('adjustment'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const StockMovementType(this.value);
  final String value;

  static StockMovementType fromJson(Object? json) {
    for (final v in StockMovementType.values) {
      if (v.value == json) return v;
    }
    return StockMovementType.$unknown;
  }

  String toJson() => value;
}

/// تسجيل دخول مواد. totalCost حقل كتابة فقط، ولا يعود في أي DTO لمسؤول المخزن.
class StockReceiveInputDto {
  const StockReceiveInputDto({
    required this.itemId,
    required this.quantity,
    this.totalCost,
    this.stockIssueId,
    this.note,
  });

  /// الصنف.
  final String itemId;
  /// الكمية الداخلة، أكبر من صفر.
  final double quantity;
  /// إجمالي تكلفة الشراء. عند وجوده ينشأ مصروف عند المحاسب. مطلوب إذا وُجد stockIssueId.
  final double? totalCost;
  /// طلب الصرف الذي اشتُريت له المواد. يجب أن تكون حالته purchasing وأن يطابق الصنف.
  final String? stockIssueId;
  /// ملاحظة.
  final String? note;

  factory StockReceiveInputDto.fromJson(Map<String, dynamic> json) => StockReceiveInputDto(
        itemId: (json['itemId']) as String,
        quantity: ((json['quantity']) as num).toDouble(),
        totalCost: json['totalCost'] == null ? null : ((json['totalCost']) as num).toDouble(),
        stockIssueId: json['stockIssueId'] == null ? null : (json['stockIssueId']) as String,
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'itemId': itemId,
        'quantity': quantity,
        if (totalCost != null) 'totalCost': totalCost!,
        if (stockIssueId != null) 'stockIssueId': stockIssueId!,
        if (note != null) 'note': note!,
      };
}

/// تصحيح رصيد بحركة عكسية.
class StockAdjustInputDto {
  const StockAdjustInputDto({
    required this.itemId,
    required this.quantity,
    required this.reason,
    this.reversesMovementId,
  });

  /// الصنف.
  final String itemId;
  /// الكمية بإشارة: موجبة تزيد الرصيد وسالبة تنقصه. لا تكون صفرًا.
  final double quantity;
  /// سبب التصحيح.
  final String reason;
  /// الحركة التي يعكسها التصحيح، إن وُجدت.
  final String? reversesMovementId;

  factory StockAdjustInputDto.fromJson(Map<String, dynamic> json) => StockAdjustInputDto(
        itemId: (json['itemId']) as String,
        quantity: ((json['quantity']) as num).toDouble(),
        reason: (json['reason']) as String,
        reversesMovementId: json['reversesMovementId'] == null ? null : (json['reversesMovementId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'itemId': itemId,
        'quantity': quantity,
        'reason': reason,
        if (reversesMovementId != null) 'reversesMovementId': reversesMovementId!,
      };
}

/// حركة مخزن. لا تُعدل ولا تُحذف، ولا تحوي تكلفة لأن مسؤول المخزن هو قارئها.
class StockMovementDto {
  const StockMovementDto({
    required this.id,
    required this.item,
    required this.type,
    required this.quantity,
    required this.balanceAfter,
    this.requestId,
    this.stockIssueId,
    this.custodyId,
    this.reversesMovementId,
    this.employee,
    required this.performedBy,
    this.note,
    required this.creationTime,
  });

  /// معرف الحركة.
  final String id;
  final ItemRefDto item;
  final StockMovementType type;
  /// الكمية بإشارة: موجبة للدخول والإرجاع، وسالبة للتسليم.
  final double quantity;
  /// رصيد الصنف بعد الحركة.
  final double balanceAfter;
  /// الطلب المرتبط.
  final String? requestId;
  /// الصرف المرتبط.
  final String? stockIssueId;
  /// العهدة المرتبطة.
  final String? custodyId;
  /// الحركة المعكوسة.
  final String? reversesMovementId;
  /// الموظف المستلم أو المرجع.
  final EmployeeRefDto? employee;
  final UserRefDto performedBy;
  /// الملاحظة أو سبب التصحيح.
  final String? note;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;

  factory StockMovementDto.fromJson(Map<String, dynamic> json) => StockMovementDto(
        id: (json['id']) as String,
        item: ItemRefDto.fromJson((json['item']) as Map<String, dynamic>),
        type: StockMovementType.fromJson(json['type']),
        quantity: ((json['quantity']) as num).toDouble(),
        balanceAfter: ((json['balanceAfter']) as num).toDouble(),
        requestId: json['requestId'] == null ? null : (json['requestId']) as String,
        stockIssueId: json['stockIssueId'] == null ? null : (json['stockIssueId']) as String,
        custodyId: json['custodyId'] == null ? null : (json['custodyId']) as String,
        reversesMovementId: json['reversesMovementId'] == null ? null : (json['reversesMovementId']) as String,
        employee: json['employee'] == null ? null : EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        performedBy: UserRefDto.fromJson((json['performedBy']) as Map<String, dynamic>),
        note: json['note'] == null ? null : (json['note']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'item': item.toJson(),
        'type': type.toJson(),
        'quantity': quantity,
        'balanceAfter': balanceAfter,
        if (requestId != null) 'requestId': requestId!,
        if (stockIssueId != null) 'stockIssueId': stockIssueId!,
        if (custodyId != null) 'custodyId': custodyId!,
        if (reversesMovementId != null) 'reversesMovementId': reversesMovementId!,
        if (employee != null) 'employee': employee!.toJson(),
        'performedBy': performedBy.toJson(),
        if (note != null) 'note': note!,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

/// صفحة من StockMovementDto.
class StockMovementPagedResultDto {
  const StockMovementPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<StockMovementDto> items;

  factory StockMovementPagedResultDto.fromJson(Map<String, dynamic> json) => StockMovementPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => StockMovementDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// pendingAcknowledgement: وصل إلى مسؤول المخزن ولم يطلع. acknowledged: «اطلعت». purchasing: «بانتظار الشراء». delivered: «تم التسليم». cancelled: انتهى لأن الطلب رُفض أو أُلغي قبل التسليم.
enum StockIssueStatus {
  pendingAcknowledgement('pendingAcknowledgement'),
  acknowledged('acknowledged'),
  purchasing('purchasing'),
  delivered('delivered'),
  cancelled('cancelled'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const StockIssueStatus(this.value);
  final String value;

  static StockIssueStatus fromJson(Object? json) {
    for (final v in StockIssueStatus.values) {
      if (v.value == json) return v;
    }
    return StockIssueStatus.$unknown;
  }

  String toJson() => value;
}

/// personal: شخصية باسم موظف. shared: مشتركة باسم قسم.
enum CustodyKind {
  personal('personal'),
  shared('shared'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const CustodyKind(this.value);
  final String value;

  static CustodyKind fromJson(Object? json) {
    for (final v in CustodyKind.values) {
      if (v.value == json) return v;
    }
    return CustodyKind.$unknown;
  }

  String toJson() => value;
}

/// pendingAcknowledgement: بانتظار تأكيد الاستلام. acknowledged: تأكد الاستلام. returned: أُرجعت.
enum CustodyStatus {
  pendingAcknowledgement('pendingAcknowledgement'),
  acknowledged('acknowledged'),
  returned('returned'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const CustodyStatus(this.value);
  final String value;

  static CustodyStatus fromJson(Object? json) {
    for (final v in CustodyStatus.values) {
      if (v.value == json) return v;
    }
    return CustodyStatus.$unknown;
  }

  String toJson() => value;
}

/// حالة الصرف كما تظهر داخل تفاصيل الطلب. دون أرصدة المخزن.
class StockIssueStatusDto {
  const StockIssueStatusDto({
    required this.id,
    required this.item,
    required this.quantity,
    required this.status,
    required this.statusLabel,
    this.deliveredAt,
    this.custodyId,
  });

  /// معرف الصرف.
  final String id;
  final ItemRefDto item;
  /// الكمية المطلوبة.
  final double quantity;
  final StockIssueStatus status;
  /// نص الحالة حسب Accept-Language.
  final String statusLabel;
  /// وقت التسليم.
  final DateTime? deliveredAt;
  /// العهدة الناتجة.
  final String? custodyId;

  factory StockIssueStatusDto.fromJson(Map<String, dynamic> json) => StockIssueStatusDto(
        id: (json['id']) as String,
        item: ItemRefDto.fromJson((json['item']) as Map<String, dynamic>),
        quantity: ((json['quantity']) as num).toDouble(),
        status: StockIssueStatus.fromJson(json['status']),
        statusLabel: (json['statusLabel']) as String,
        deliveredAt: json['deliveredAt'] == null ? null : DateTime.parse((json['deliveredAt']) as String),
        custodyId: json['custodyId'] == null ? null : (json['custodyId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'item': item.toJson(),
        'quantity': quantity,
        'status': status.toJson(),
        'statusLabel': statusLabel,
        if (deliveredAt != null) 'deliveredAt': hrDateTimeToJson(deliveredAt!),
        if (custodyId != null) 'custodyId': custodyId!,
      };
}

/// طلب صرف لمسؤول المخزن. الموظف عبر EmployeeRefDto فقط.
class StockIssueDto {
  const StockIssueDto({
    required this.id,
    required this.requestId,
    required this.item,
    required this.quantity,
    required this.quantityOnHand,
    required this.isAvailable,
    required this.status,
    required this.statusLabel,
    required this.employee,
    this.acknowledgedAt,
    this.acknowledgedBy,
    this.purchasingAt,
    this.deliveredAt,
    this.deliveredBy,
    this.custodyId,
    this.stockMovementId,
    required this.creationTime,
  });

  /// معرف الصرف.
  final String id;
  /// الطلب الأصلي.
  final String requestId;
  final ItemRefDto item;
  /// الكمية المطلوبة.
  final double quantity;
  /// رصيد الصنف الآن.
  final double quantityOnHand;
  /// quantityOnHand لا يقل عن quantity.
  final bool isAvailable;
  final StockIssueStatus status;
  /// نص الحالة حسب Accept-Language.
  final String statusLabel;
  /// صاحب الطلب: الاسم والقسم فقط.
  final EmployeeRefDto employee;
  /// وقت «اطلعت».
  final DateTime? acknowledgedAt;
  /// من أشر «اطلعت».
  final UserRefDto? acknowledgedBy;
  /// وقت «بانتظار الشراء».
  final DateTime? purchasingAt;
  /// وقت التسليم.
  final DateTime? deliveredAt;
  /// من سلم.
  final UserRefDto? deliveredBy;
  /// العهدة الناتجة. null للمستهلك وقبل التسليم.
  final String? custodyId;
  /// حركة الخصم بعد التسليم.
  final String? stockMovementId;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;

  factory StockIssueDto.fromJson(Map<String, dynamic> json) => StockIssueDto(
        id: (json['id']) as String,
        requestId: (json['requestId']) as String,
        item: ItemRefDto.fromJson((json['item']) as Map<String, dynamic>),
        quantity: ((json['quantity']) as num).toDouble(),
        quantityOnHand: ((json['quantityOnHand']) as num).toDouble(),
        isAvailable: (json['isAvailable']) as bool,
        status: StockIssueStatus.fromJson(json['status']),
        statusLabel: (json['statusLabel']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
        acknowledgedBy: json['acknowledgedBy'] == null ? null : UserRefDto.fromJson((json['acknowledgedBy']) as Map<String, dynamic>),
        purchasingAt: json['purchasingAt'] == null ? null : DateTime.parse((json['purchasingAt']) as String),
        deliveredAt: json['deliveredAt'] == null ? null : DateTime.parse((json['deliveredAt']) as String),
        deliveredBy: json['deliveredBy'] == null ? null : UserRefDto.fromJson((json['deliveredBy']) as Map<String, dynamic>),
        custodyId: json['custodyId'] == null ? null : (json['custodyId']) as String,
        stockMovementId: json['stockMovementId'] == null ? null : (json['stockMovementId']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'requestId': requestId,
        'item': item.toJson(),
        'quantity': quantity,
        'quantityOnHand': quantityOnHand,
        'isAvailable': isAvailable,
        'status': status.toJson(),
        'statusLabel': statusLabel,
        'employee': employee.toJson(),
        if (acknowledgedAt != null) 'acknowledgedAt': hrDateTimeToJson(acknowledgedAt!),
        if (acknowledgedBy != null) 'acknowledgedBy': acknowledgedBy!.toJson(),
        if (purchasingAt != null) 'purchasingAt': hrDateTimeToJson(purchasingAt!),
        if (deliveredAt != null) 'deliveredAt': hrDateTimeToJson(deliveredAt!),
        if (deliveredBy != null) 'deliveredBy': deliveredBy!.toJson(),
        if (custodyId != null) 'custodyId': custodyId!,
        if (stockMovementId != null) 'stockMovementId': stockMovementId!,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

/// صفحة من StockIssueDto.
class StockIssuePagedResultDto {
  const StockIssuePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<StockIssueDto> items;

  factory StockIssuePagedResultDto.fromJson(Map<String, dynamic> json) => StockIssuePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => StockIssueDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// تسليم الصنف.
class StockIssueDeliverInputDto {
  const StockIssueDeliverInputDto({
    this.custodyKind,
    this.note,
  });

  /// للصنف الذي يُسترد فقط. null تعني personal باسم صاحب الطلب. shared تنشئ العهدة باسم قسم صاحب الطلب. تُهمل للمستهلك.
  final CustodyKind? custodyKind;
  /// ملاحظة التسليم.
  final String? note;

  factory StockIssueDeliverInputDto.fromJson(Map<String, dynamic> json) => StockIssueDeliverInputDto(
        custodyKind: json['custodyKind'] == null ? null : CustodyKind.fromJson(json['custodyKind']),
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (custodyKind != null) 'custodyKind': custodyKind!.toJson(),
        if (note != null) 'note': note!,
      };
}

/// إضافة عهدة مباشرة. personal: employeeId مطلوب وdepartmentId = null. shared: departmentId مطلوب وemployeeId = null.
class CustodyCreateInputDto {
  const CustodyCreateInputDto({
    required this.itemId,
    required this.kind,
    this.employeeId,
    this.departmentId,
    required this.quantity,
    required this.deductFromStock,
    this.note,
  });

  /// صنف من نوع returnable.
  final String itemId;
  final CustodyKind kind;
  /// الموظف، من GET /api/app/employee/lookup.
  final String? employeeId;
  /// القسم، من GET /api/app/department.
  final String? departmentId;
  /// الكمية، أكبر من صفر.
  final double quantity;
  /// true: تسليم فعلي الآن من رصيد المخزن بحركة issue. false: تسجيل عهدة موجودة أصلًا عند الموظف أو القسم دون حركة.
  final bool deductFromStock;
  /// ملاحظة.
  final String? note;

  factory CustodyCreateInputDto.fromJson(Map<String, dynamic> json) => CustodyCreateInputDto(
        itemId: (json['itemId']) as String,
        kind: CustodyKind.fromJson(json['kind']),
        employeeId: json['employeeId'] == null ? null : (json['employeeId']) as String,
        departmentId: json['departmentId'] == null ? null : (json['departmentId']) as String,
        quantity: ((json['quantity']) as num).toDouble(),
        deductFromStock: (json['deductFromStock']) as bool,
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'itemId': itemId,
        'kind': kind.toJson(),
        if (employeeId != null) 'employeeId': employeeId!,
        if (departmentId != null) 'departmentId': departmentId!,
        'quantity': quantity,
        'deductFromStock': deductFromStock,
        if (note != null) 'note': note!,
      };
}

/// عهدة. تحمل employee أو department، وليس الاثنين معًا: employee غير null فقط عندما kind = personal، وdepartment غير null فقط عندما kind = shared.
class CustodyDto {
  const CustodyDto({
    required this.id,
    required this.item,
    required this.kind,
    this.employee,
    this.department,
    required this.quantity,
    required this.status,
    required this.assignedAt,
    required this.assignedBy,
    this.acknowledgedAt,
    this.acknowledgedBy,
    this.returnedAt,
    this.requestId,
    this.stockIssueId,
    this.note,
  });

  /// معرف العهدة.
  final String id;
  final ItemRefDto item;
  final CustodyKind kind;
  /// صاحب العهدة الشخصية.
  final EmployeeRefDto? employee;
  /// قسم العهدة المشتركة.
  final DepartmentRefDto? department;
  /// الكمية.
  final double quantity;
  final CustodyStatus status;
  /// وقت التسجيل.
  final DateTime assignedAt;
  final UserRefDto assignedBy;
  /// وقت تأكيد الاستلام.
  final DateTime? acknowledgedAt;
  /// من أكد الاستلام.
  final UserRefDto? acknowledgedBy;
  /// وقت الإرجاع.
  final DateTime? returnedAt;
  /// الطلب المصدر.
  final String? requestId;
  /// الصرف المصدر. null للعهدة المباشرة.
  final String? stockIssueId;
  /// ملاحظة.
  final String? note;

  factory CustodyDto.fromJson(Map<String, dynamic> json) => CustodyDto(
        id: (json['id']) as String,
        item: ItemRefDto.fromJson((json['item']) as Map<String, dynamic>),
        kind: CustodyKind.fromJson(json['kind']),
        employee: json['employee'] == null ? null : EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        department: json['department'] == null ? null : DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        quantity: ((json['quantity']) as num).toDouble(),
        status: CustodyStatus.fromJson(json['status']),
        assignedAt: DateTime.parse((json['assignedAt']) as String),
        assignedBy: UserRefDto.fromJson((json['assignedBy']) as Map<String, dynamic>),
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
        acknowledgedBy: json['acknowledgedBy'] == null ? null : UserRefDto.fromJson((json['acknowledgedBy']) as Map<String, dynamic>),
        returnedAt: json['returnedAt'] == null ? null : DateTime.parse((json['returnedAt']) as String),
        requestId: json['requestId'] == null ? null : (json['requestId']) as String,
        stockIssueId: json['stockIssueId'] == null ? null : (json['stockIssueId']) as String,
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'item': item.toJson(),
        'kind': kind.toJson(),
        if (employee != null) 'employee': employee!.toJson(),
        if (department != null) 'department': department!.toJson(),
        'quantity': quantity,
        'status': status.toJson(),
        'assignedAt': hrDateTimeToJson(assignedAt),
        'assignedBy': assignedBy.toJson(),
        if (acknowledgedAt != null) 'acknowledgedAt': hrDateTimeToJson(acknowledgedAt!),
        if (acknowledgedBy != null) 'acknowledgedBy': acknowledgedBy!.toJson(),
        if (returnedAt != null) 'returnedAt': hrDateTimeToJson(returnedAt!),
        if (requestId != null) 'requestId': requestId!,
        if (stockIssueId != null) 'stockIssueId': stockIssueId!,
        if (note != null) 'note': note!,
      };
}

/// صفحة من CustodyDto.
class CustodyPagedResultDto {
  const CustodyPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<CustodyDto> items;

  factory CustodyPagedResultDto.fromJson(Map<String, dynamic> json) => CustodyPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => CustodyDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// إرجاع عهدة كاملة.
class CustodyReturnInputDto {
  const CustodyReturnInputDto({
    required this.restock,
    this.note,
  });

  /// true: تعود الكمية إلى الرصيد بحركة return. false: لا تعود، مثل التلف، ويصبح note مطلوبًا.
  final bool restock;
  /// ملاحظة الإرجاع.
  final String? note;

  factory CustodyReturnInputDto.fromJson(Map<String, dynamic> json) => CustodyReturnInputDto(
        restock: (json['restock']) as bool,
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'restock': restock,
        if (note != null) 'note': note!,
      };
}

/// matched: كل الكميات مطابقة. discrepancy: يوجد فرق.
enum CustodyCountResult {
  matched('matched'),
  discrepancy('discrepancy'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const CustodyCountResult(this.value);
  final String value;

  static CustodyCountResult fromJson(Object? json) {
    for (final v in CustodyCountResult.values) {
      if (v.value == json) return v;
    }
    return CustodyCountResult.$unknown;
  }

  String toJson() => value;
}

/// سطر جرد.
class CustodyCountLineInputDto {
  const CustodyCountLineInputDto({
    required this.custodyId,
    required this.countedQuantity,
    this.note,
  });

  /// عهدة مشتركة قائمة في القسم.
  final String custodyId;
  /// الكمية الموجودة فعليًا.
  final double countedQuantity;
  /// ملاحظة السطر.
  final String? note;

  factory CustodyCountLineInputDto.fromJson(Map<String, dynamic> json) => CustodyCountLineInputDto(
        custodyId: (json['custodyId']) as String,
        countedQuantity: ((json['countedQuantity']) as num).toDouble(),
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'custodyId': custodyId,
        'countedQuantity': countedQuantity,
        if (note != null) 'note': note!,
      };
}

/// جرد عهدة قسم.
class CustodyCountCreateInputDto {
  const CustodyCountCreateInputDto({
    required this.departmentId,
    required this.countDate,
    this.note,
    required this.lines,
  });

  /// القسم.
  final String departmentId;
  /// تاريخ الجرد.
  final DateTime countDate;
  /// ملاحظة عامة.
  final String? note;
  /// سطر لكل عهدة مشتركة قائمة في القسم.
  final List<CustodyCountLineInputDto> lines;

  factory CustodyCountCreateInputDto.fromJson(Map<String, dynamic> json) => CustodyCountCreateInputDto(
        departmentId: (json['departmentId']) as String,
        countDate: hrDateFromJson((json['countDate']) as String),
        note: json['note'] == null ? null : (json['note']) as String,
        lines: ((json['lines']) as List).map((e) => CustodyCountLineInputDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'departmentId': departmentId,
        'countDate': hrDateToJson(countDate),
        if (note != null) 'note': note!,
        'lines': lines.map((e) => e.toJson()).toList(),
      };
}

/// سطر جرد محفوظ.
class CustodyCountLineDto {
  const CustodyCountLineDto({
    required this.custodyId,
    required this.item,
    required this.expectedQuantity,
    required this.countedQuantity,
    this.note,
  });

  /// العهدة.
  final String custodyId;
  final ItemRefDto item;
  /// الكمية المسجلة وقت الجرد.
  final double expectedQuantity;
  /// الكمية الموجودة فعليًا.
  final double countedQuantity;
  /// ملاحظة السطر.
  final String? note;

  factory CustodyCountLineDto.fromJson(Map<String, dynamic> json) => CustodyCountLineDto(
        custodyId: (json['custodyId']) as String,
        item: ItemRefDto.fromJson((json['item']) as Map<String, dynamic>),
        expectedQuantity: ((json['expectedQuantity']) as num).toDouble(),
        countedQuantity: ((json['countedQuantity']) as num).toDouble(),
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'custodyId': custodyId,
        'item': item.toJson(),
        'expectedQuantity': expectedQuantity,
        'countedQuantity': countedQuantity,
        if (note != null) 'note': note!,
      };
}

/// جرد عهدة قسم. لا يغير العهد ولا الرصيد.
class CustodyCountDto {
  const CustodyCountDto({
    required this.id,
    required this.department,
    required this.countDate,
    required this.result,
    this.note,
    required this.countedBy,
    required this.lines,
    required this.creationTime,
  });

  /// معرف الجرد.
  final String id;
  final DepartmentRefDto department;
  /// تاريخ الجرد.
  final DateTime countDate;
  final CustodyCountResult result;
  /// ملاحظة عامة.
  final String? note;
  final UserRefDto countedBy;
  /// أسطر الجرد.
  final List<CustodyCountLineDto> lines;
  /// وقت الإنشاء UTC.
  final DateTime creationTime;

  factory CustodyCountDto.fromJson(Map<String, dynamic> json) => CustodyCountDto(
        id: (json['id']) as String,
        department: DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        countDate: hrDateFromJson((json['countDate']) as String),
        result: CustodyCountResult.fromJson(json['result']),
        note: json['note'] == null ? null : (json['note']) as String,
        countedBy: UserRefDto.fromJson((json['countedBy']) as Map<String, dynamic>),
        lines: ((json['lines']) as List).map((e) => CustodyCountLineDto.fromJson((e) as Map<String, dynamic>)).toList(),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'department': department.toJson(),
        'countDate': hrDateToJson(countDate),
        'result': result.toJson(),
        if (note != null) 'note': note!,
        'countedBy': countedBy.toJson(),
        'lines': lines.map((e) => e.toJson()).toList(),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

/// صفحة من CustodyCountDto.
class CustodyCountPagedResultDto {
  const CustodyCountPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<CustodyCountDto> items;

  factory CustodyCountPagedResultDto.fromJson(Map<String, dynamic> json) => CustodyCountPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => CustodyCountDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// مستوى الشكوى. الترتيب من الأدنى إلى الأعلى: normal ثم confidential ثم highlyConfidential.
enum ComplaintLevel {
  normal('normal'),
  confidential('confidential'),
  highlyConfidential('highlyConfidential'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintLevel(this.value);
  final String value;

  static ComplaintLevel fromJson(Object? json) {
    for (final v in ComplaintLevel.values) {
      if (v.value == json) return v;
    }
    return ComplaintLevel.$unknown;
  }

  String toJson() => value;
}

/// حالة الشكوى. الحالات المفتوحة تحدد الجهة التي تقف عندها الشكوى.
enum ComplaintStatus {
  pendingDepartmentManager('pendingDepartmentManager'),
  pendingHrManager('pendingHrManager'),
  pendingExecutiveDirector('pendingExecutiveDirector'),
  closed('closed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintStatus(this.value);
  final String value;

  static ComplaintStatus fromJson(Object? json) {
    for (final v in ComplaintStatus.values) {
      if (v.value == json) return v;
    }
    return ComplaintStatus.$unknown;
  }

  String toJson() => value;
}

/// employee: ضد شخص محدد. general: شكوى عامة دون مشتكى عليه.
enum ComplaintTargetKind {
  employee('employee'),
  general('general'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintTargetKind(this.value);
  final String value;

  static ComplaintTargetKind fromJson(Object? json) {
    for (final v in ComplaintTargetKind.values) {
      if (v.value == json) return v;
    }
    return ComplaintTargetKind.$unknown;
  }

  String toJson() => value;
}

/// فئة المشتكى عليه كما يحددها الخادم. لا يراها إلا المعالج.
enum ComplaintRespondentCategory {
  colleague('colleague'),
  departmentManager('departmentManager'),
  storeOfficer('storeOfficer'),
  hrManager('hrManager'),
  reviewer('reviewer'),
  accountant('accountant'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintRespondentCategory(this.value);
  final String value;

  static ComplaintRespondentCategory fromJson(Object? json) {
    for (final v in ComplaintRespondentCategory.values) {
      if (v.value == json) return v;
    }
    return ComplaintRespondentCategory.$unknown;
  }

  String toJson() => value;
}

/// الجهة المعالجة في مسار الشكوى.
enum ComplaintHandlerRole {
  departmentManager('departmentManager'),
  hrManager('hrManager'),
  executiveDirector('executiveDirector'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintHandlerRole(this.value);
  final String value;

  static ComplaintHandlerRole fromJson(Object? json) {
    for (final v in ComplaintHandlerRole.values) {
      if (v.value == json) return v;
    }
    return ComplaintHandlerRole.$unknown;
  }

  String toJson() => value;
}

/// أنواع الإجراءات التي يضيفها المعالج يدويًا.
enum ComplaintManualActionType {
  note('note'),
  meeting('meeting'),
  investigation('investigation'),
  followUp('followUp'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintManualActionType(this.value);
  final String value;

  static ComplaintManualActionType fromJson(Object? json) {
    for (final v in ComplaintManualActionType.values) {
      if (v.value == json) return v;
    }
    return ComplaintManualActionType.$unknown;
  }

  String toJson() => value;
}

/// كل أنواع سجل الإجراءات. submitted وlevelRaised وforwarded وclosed يولدها الخادم.
enum ComplaintActionType {
  note('note'),
  meeting('meeting'),
  investigation('investigation'),
  followUp('followUp'),
  submitted('submitted'),
  levelRaised('levelRaised'),
  forwarded('forwarded'),
  closed('closed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintActionType(this.value);
  final String value;

  static ComplaintActionType fromJson(Object? json) {
    for (final v in ComplaintActionType.values) {
      if (v.value == json) return v;
    }
    return ComplaintActionType.$unknown;
  }

  String toJson() => value;
}

/// نوع العملية في سجل الاطلاع.
enum ComplaintAccessAction {
  viewDetail('viewDetail'),
  downloadAttachment('downloadAttachment'),
  uploadAttachment('uploadAttachment'),
  viewAccessLog('viewAccessLog'),
  addAction('addAction'),
  raiseLevel('raiseLevel'),
  forward('forward'),
  close('close'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintAccessAction(this.value);
  final String value;

  static ComplaintAccessAction fromJson(Object? json) {
    for (final v in ComplaintAccessAction.values) {
      if (v.value == json) return v;
    }
    return ComplaintAccessAction.$unknown;
  }

  String toJson() => value;
}

/// حالة طلب تعيين معالج.
enum ComplaintHandlerAssignmentStatus {
  pendingApproval('pendingApproval'),
  approved('approved'),
  rejected('rejected'),
  revoked('revoked'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ComplaintHandlerAssignmentStatus(this.value);
  final String value;

  static ComplaintHandlerAssignmentStatus fromJson(Object? json) {
    for (final v in ComplaintHandlerAssignmentStatus.values) {
      if (v.value == json) return v;
    }
    return ComplaintHandlerAssignmentStatus.$unknown;
  }

  String toJson() => value;
}

/// قائمة المشتكى عليهم الممكنين. لا تحتوي دورًا أو فئة حتى لا تكشف الهيكل الداخلي للتوجيه.
class ComplaintRespondentPagedResultDto {
  const ComplaintRespondentPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EmployeeRefDto> items;

  factory ComplaintRespondentPagedResultDto.fromJson(Map<String, dynamic> json) => ComplaintRespondentPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EmployeeRefDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class ComplaintRoutingPreviewInputDto {
  const ComplaintRoutingPreviewInputDto({
    required this.targetKind,
    this.respondentId,
  });

  final ComplaintTargetKind targetKind;
  /// مطلوب عندما تكون targetKind = employee، ويجب أن يكون null أو غائبًا عندما تكون general.
  final String? respondentId;

  factory ComplaintRoutingPreviewInputDto.fromJson(Map<String, dynamic> json) => ComplaintRoutingPreviewInputDto(
        targetKind: ComplaintTargetKind.fromJson(json['targetKind']),
        respondentId: json['respondentId'] == null ? null : (json['respondentId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'targetKind': targetKind.toJson(),
        if (respondentId != null) 'respondentId': respondentId!,
      };
}

/// المسار الذي ستسلكه الشكوى إذا قُدمت بهذا المستوى.
class ComplaintRouteOptionDto {
  const ComplaintRouteOptionDto({
    required this.level,
    required this.handlerPath,
  });

  final ComplaintLevel level;
  /// الجهات بالترتيب. الأدوار فقط، دون أسماء أشخاص.
  final List<ComplaintHandlerRole> handlerPath;

  factory ComplaintRouteOptionDto.fromJson(Map<String, dynamic> json) => ComplaintRouteOptionDto(
        level: ComplaintLevel.fromJson(json['level']),
        handlerPath: ((json['handlerPath']) as List).map((e) => ComplaintHandlerRole.fromJson(e)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'level': level.toJson(),
        'handlerPath': handlerPath.map((e) => e.toJson()).toList(),
      };
}

class ComplaintRoutingPreviewDto {
  const ComplaintRoutingPreviewDto({
    required this.minimumLevel,
    required this.selectableLevels,
    required this.routes,
  });

  /// أدنى مستوى تفرضه هوية المشتكى عليه.
  final ComplaintLevel minimumLevel;
  /// المستويات التي تظهر للموظف، من minimumLevel فأعلى.
  final List<ComplaintLevel> selectableLevels;
  /// عنصر واحد لكل مستوى في selectableLevels.
  final List<ComplaintRouteOptionDto> routes;

  factory ComplaintRoutingPreviewDto.fromJson(Map<String, dynamic> json) => ComplaintRoutingPreviewDto(
        minimumLevel: ComplaintLevel.fromJson(json['minimumLevel']),
        selectableLevels: ((json['selectableLevels']) as List).map((e) => ComplaintLevel.fromJson(e)).toList(),
        routes: ((json['routes']) as List).map((e) => ComplaintRouteOptionDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'minimumLevel': minimumLevel.toJson(),
        'selectableLevels': selectableLevels.map((e) => e.toJson()).toList(),
        'routes': routes.map((e) => e.toJson()).toList(),
      };
}

class ComplaintCreateDto {
  const ComplaintCreateDto({
    required this.targetKind,
    this.respondentId,
    required this.level,
    required this.subject,
    required this.content,
  });

  final ComplaintTargetKind targetKind;
  /// مطلوب عندما تكون targetKind = employee، ويجب أن يكون null أو غائبًا عندما تكون general.
  final String? respondentId;
  /// المستوى الذي يختاره الموظف. إذا كان أقل من الحد الأدنى يرفعه الخادم تلقائيًا.
  final ComplaintLevel level;
  final String subject;
  final String content;

  factory ComplaintCreateDto.fromJson(Map<String, dynamic> json) => ComplaintCreateDto(
        targetKind: ComplaintTargetKind.fromJson(json['targetKind']),
        respondentId: json['respondentId'] == null ? null : (json['respondentId']) as String,
        level: ComplaintLevel.fromJson(json['level']),
        subject: (json['subject']) as String,
        content: (json['content']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'targetKind': targetKind.toJson(),
        if (respondentId != null) 'respondentId': respondentId!,
        'level': level.toJson(),
        'subject': subject,
        'content': content,
      };
}

class ComplaintSubmitResultDto {
  const ComplaintSubmitResultDto({
    required this.id,
    required this.referenceNumber,
    required this.level,
    required this.levelRaisedBySystem,
    required this.status,
    required this.submittedAt,
  });

  final String id;
  /// رقم مرجعي يعرض في الواجهة، لا يحمل معلومات عن المستوى أو الأطراف.
  final String referenceNumber;
  /// المستوى الفعلي بعد تطبيق الحد الأدنى.
  final ComplaintLevel level;
  /// true إذا رفع الخادم المستوى عن اختيار الموظف.
  final bool levelRaisedBySystem;
  final ComplaintStatus status;
  final DateTime submittedAt;

  factory ComplaintSubmitResultDto.fromJson(Map<String, dynamic> json) => ComplaintSubmitResultDto(
        id: (json['id']) as String,
        referenceNumber: (json['referenceNumber']) as String,
        level: ComplaintLevel.fromJson(json['level']),
        levelRaisedBySystem: (json['levelRaisedBySystem']) as bool,
        status: ComplaintStatus.fromJson(json['status']),
        submittedAt: DateTime.parse((json['submittedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'referenceNumber': referenceNumber,
        'level': level.toJson(),
        'levelRaisedBySystem': levelRaisedBySystem,
        'status': status.toJson(),
        'submittedAt': hrDateTimeToJson(submittedAt),
      };
}

/// عنصر في شكاواي. لا يحتوي بيانات المعالجة الداخلية.
class ComplaintComplainantSummaryDto {
  const ComplaintComplainantSummaryDto({
    required this.id,
    required this.referenceNumber,
    required this.subject,
    required this.level,
    required this.status,
    required this.submittedAt,
    this.closedAt,
  });

  final String id;
  final String referenceNumber;
  final String subject;
  final ComplaintLevel level;
  final ComplaintStatus status;
  final DateTime submittedAt;
  final DateTime? closedAt;

  factory ComplaintComplainantSummaryDto.fromJson(Map<String, dynamic> json) => ComplaintComplainantSummaryDto(
        id: (json['id']) as String,
        referenceNumber: (json['referenceNumber']) as String,
        subject: (json['subject']) as String,
        level: ComplaintLevel.fromJson(json['level']),
        status: ComplaintStatus.fromJson(json['status']),
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        closedAt: json['closedAt'] == null ? null : DateTime.parse((json['closedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'referenceNumber': referenceNumber,
        'subject': subject,
        'level': level.toJson(),
        'status': status.toJson(),
        'submittedAt': hrDateTimeToJson(submittedAt),
        'closedAt': closedAt == null ? null : hrDateTimeToJson(closedAt!),
      };
}

class ComplaintComplainantSummaryPagedResultDto {
  const ComplaintComplainantSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ComplaintComplainantSummaryDto> items;

  factory ComplaintComplainantSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => ComplaintComplainantSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ComplaintComplainantSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// عرض مقدم الشكوى. لا يحتوي الإجراءات الداخلية ولا المعالج ولا سجل الاطلاع ولا المهلة ولا فئة المشتكى عليه. الحقول غائبة وليست null.
class ComplaintComplainantDetailDto {
  const ComplaintComplainantDetailDto({
    this.closureDecision,
    required this.id,
    required this.referenceNumber,
    required this.targetKind,
    this.respondent,
    required this.level,
    required this.status,
    required this.subject,
    required this.content,
    required this.attachments,
    required this.submittedAt,
    this.closedAt,
    required this.concurrencyStamp,
  });

  /// The final decision, once the complaint is closed.
  final String? closureDecision;
  final String id;
  final String referenceNumber;
  final ComplaintTargetKind targetKind;
  final EmployeeRefDto? respondent;
  final ComplaintLevel level;
  final ComplaintStatus status;
  final String subject;
  final String content;
  /// مرفقات مقدم الشكوى فقط، ولا تشمل مرفقات أضافها المعالج.
  final List<AttachmentDto> attachments;
  final DateTime submittedAt;
  final DateTime? closedAt;
  final String concurrencyStamp;

  factory ComplaintComplainantDetailDto.fromJson(Map<String, dynamic> json) => ComplaintComplainantDetailDto(
        closureDecision: json['closureDecision'] == null ? null : (json['closureDecision']) as String,
        id: (json['id']) as String,
        referenceNumber: (json['referenceNumber']) as String,
        targetKind: ComplaintTargetKind.fromJson(json['targetKind']),
        respondent: json['respondent'] == null ? null : EmployeeRefDto.fromJson((json['respondent']) as Map<String, dynamic>),
        level: ComplaintLevel.fromJson(json['level']),
        status: ComplaintStatus.fromJson(json['status']),
        subject: (json['subject']) as String,
        content: (json['content']) as String,
        attachments: ((json['attachments']) as List).map((e) => AttachmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        closedAt: json['closedAt'] == null ? null : DateTime.parse((json['closedAt']) as String),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'closureDecision': closureDecision == null ? null : closureDecision!,
        'id': id,
        'referenceNumber': referenceNumber,
        'targetKind': targetKind.toJson(),
        'respondent': respondent == null ? null : respondent!.toJson(),
        'level': level.toJson(),
        'status': status.toJson(),
        'subject': subject,
        'content': content,
        'attachments': attachments.map((e) => e.toJson()).toList(),
        'submittedAt': hrDateTimeToJson(submittedAt),
        'closedAt': closedAt == null ? null : hrDateTimeToJson(closedAt!),
        'concurrencyStamp': concurrencyStamp,
      };
}

/// عنصر في صندوق المعالج. لا يحتوي الموضوع ولا أسماء الأطراف، لأن قراءتها اطلاع يجب تسجيله عبر التفاصيل.
class ComplaintHandlerSummaryDto {
  const ComplaintHandlerSummaryDto({
    required this.id,
    required this.referenceNumber,
    required this.level,
    required this.status,
    required this.submittedAt,
    this.dueAt,
    required this.isLate,
    this.closedAt,
  });

  final String id;
  final String referenceNumber;
  final ComplaintLevel level;
  final ComplaintStatus status;
  final DateTime submittedAt;
  /// فارغ إلى أن يحدد مسؤول النظام مهلة الشكوى.
  final DateTime? dueAt;
  final bool isLate;
  final DateTime? closedAt;

  factory ComplaintHandlerSummaryDto.fromJson(Map<String, dynamic> json) => ComplaintHandlerSummaryDto(
        id: (json['id']) as String,
        referenceNumber: (json['referenceNumber']) as String,
        level: ComplaintLevel.fromJson(json['level']),
        status: ComplaintStatus.fromJson(json['status']),
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        dueAt: json['dueAt'] == null ? null : DateTime.parse((json['dueAt']) as String),
        isLate: (json['isLate']) as bool,
        closedAt: json['closedAt'] == null ? null : DateTime.parse((json['closedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'referenceNumber': referenceNumber,
        'level': level.toJson(),
        'status': status.toJson(),
        'submittedAt': hrDateTimeToJson(submittedAt),
        'dueAt': dueAt == null ? null : hrDateTimeToJson(dueAt!),
        'isLate': isLate,
        'closedAt': closedAt == null ? null : hrDateTimeToJson(closedAt!),
      };
}

class ComplaintHandlerSummaryPagedResultDto {
  const ComplaintHandlerSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ComplaintHandlerSummaryDto> items;

  factory ComplaintHandlerSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => ComplaintHandlerSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ComplaintHandlerSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// إجراء في سجل المعالجة. للمعالج فقط.
class ComplaintActionDto {
  const ComplaintActionDto({
    required this.id,
    required this.type,
    required this.actor,
    this.note,
    required this.creationTime,
  });

  final String id;
  final ComplaintActionType type;
  final UserRefDto actor;
  final String? note;
  final DateTime creationTime;

  factory ComplaintActionDto.fromJson(Map<String, dynamic> json) => ComplaintActionDto(
        id: (json['id']) as String,
        type: ComplaintActionType.fromJson(json['type']),
        actor: UserRefDto.fromJson((json['actor']) as Map<String, dynamic>),
        note: json['note'] == null ? null : (json['note']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'type': type.toJson(),
        'actor': actor.toJson(),
        'note': note == null ? null : note!,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

/// عرض المعالج. اسم مقدم الشكوى ظاهر دائمًا، ولا توجد شكوى مجهولة.
class ComplaintHandlerDetailDto {
  const ComplaintHandlerDetailDto({
    required this.id,
    required this.referenceNumber,
    required this.complainant,
    required this.targetKind,
    this.respondent,
    this.respondentCategory,
    required this.level,
    required this.minimumLevel,
    required this.status,
    this.handler,
    required this.subject,
    required this.content,
    required this.attachments,
    required this.actions,
    required this.submittedAt,
    this.dueAt,
    required this.isLate,
    this.closedAt,
    this.closureDecision,
    required this.concurrencyStamp,
  });

  final String id;
  final String referenceNumber;
  final EmployeeRefDto complainant;
  final ComplaintTargetKind targetKind;
  final EmployeeRefDto? respondent;
  final ComplaintRespondentCategory? respondentCategory;
  final ComplaintLevel level;
  /// الحد الأدنى الذي تفرضه هوية المشتكى عليه.
  final ComplaintLevel minimumLevel;
  final ComplaintStatus status;
  /// المعالج الحالي، وnull إذا كانت الشكوى مغلقة.
  final UserRefDto? handler;
  final String subject;
  final String content;
  final List<AttachmentDto> attachments;
  final List<ComplaintActionDto> actions;
  final DateTime submittedAt;
  /// فارغ إلى أن يحدد مسؤول النظام مهلة الشكوى.
  final DateTime? dueAt;
  final bool isLate;
  final DateTime? closedAt;
  final String? closureDecision;
  final String concurrencyStamp;

  factory ComplaintHandlerDetailDto.fromJson(Map<String, dynamic> json) => ComplaintHandlerDetailDto(
        id: (json['id']) as String,
        referenceNumber: (json['referenceNumber']) as String,
        complainant: EmployeeRefDto.fromJson((json['complainant']) as Map<String, dynamic>),
        targetKind: ComplaintTargetKind.fromJson(json['targetKind']),
        respondent: json['respondent'] == null ? null : EmployeeRefDto.fromJson((json['respondent']) as Map<String, dynamic>),
        respondentCategory: json['respondentCategory'] == null ? null : ComplaintRespondentCategory.fromJson(json['respondentCategory']),
        level: ComplaintLevel.fromJson(json['level']),
        minimumLevel: ComplaintLevel.fromJson(json['minimumLevel']),
        status: ComplaintStatus.fromJson(json['status']),
        handler: json['handler'] == null ? null : UserRefDto.fromJson((json['handler']) as Map<String, dynamic>),
        subject: (json['subject']) as String,
        content: (json['content']) as String,
        attachments: ((json['attachments']) as List).map((e) => AttachmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
        actions: ((json['actions']) as List).map((e) => ComplaintActionDto.fromJson((e) as Map<String, dynamic>)).toList(),
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        dueAt: json['dueAt'] == null ? null : DateTime.parse((json['dueAt']) as String),
        isLate: (json['isLate']) as bool,
        closedAt: json['closedAt'] == null ? null : DateTime.parse((json['closedAt']) as String),
        closureDecision: json['closureDecision'] == null ? null : (json['closureDecision']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'referenceNumber': referenceNumber,
        'complainant': complainant.toJson(),
        'targetKind': targetKind.toJson(),
        'respondent': respondent == null ? null : respondent!.toJson(),
        'respondentCategory': respondentCategory == null ? null : respondentCategory!.toJson(),
        'level': level.toJson(),
        'minimumLevel': minimumLevel.toJson(),
        'status': status.toJson(),
        'handler': handler == null ? null : handler!.toJson(),
        'subject': subject,
        'content': content,
        'attachments': attachments.map((e) => e.toJson()).toList(),
        'actions': actions.map((e) => e.toJson()).toList(),
        'submittedAt': hrDateTimeToJson(submittedAt),
        'dueAt': dueAt == null ? null : hrDateTimeToJson(dueAt!),
        'isLate': isLate,
        'closedAt': closedAt == null ? null : hrDateTimeToJson(closedAt!),
        'closureDecision': closureDecision == null ? null : closureDecision!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class ComplaintRaiseLevelInputDto {
  const ComplaintRaiseLevelInputDto({
    required this.level,
    this.reason,
    required this.concurrencyStamp,
  });

  /// يجب أن يكون أعلى من المستوى الحالي.
  final ComplaintLevel level;
  final String? reason;
  final String concurrencyStamp;

  factory ComplaintRaiseLevelInputDto.fromJson(Map<String, dynamic> json) => ComplaintRaiseLevelInputDto(
        level: ComplaintLevel.fromJson(json['level']),
        reason: json['reason'] == null ? null : (json['reason']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'level': level.toJson(),
        if (reason != null) 'reason': reason!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class ComplaintActionCreateDto {
  const ComplaintActionCreateDto({
    required this.type,
    required this.note,
  });

  final ComplaintManualActionType type;
  final String note;

  factory ComplaintActionCreateDto.fromJson(Map<String, dynamic> json) => ComplaintActionCreateDto(
        type: ComplaintManualActionType.fromJson(json['type']),
        note: (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'type': type.toJson(),
        'note': note,
      };
}

class ComplaintForwardInputDto {
  const ComplaintForwardInputDto({
    this.note,
    required this.concurrencyStamp,
  });

  final String? note;
  final String concurrencyStamp;

  factory ComplaintForwardInputDto.fromJson(Map<String, dynamic> json) => ComplaintForwardInputDto(
        note: json['note'] == null ? null : (json['note']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (note != null) 'note': note!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class ComplaintCloseInputDto {
  const ComplaintCloseInputDto({
    required this.decision,
    required this.concurrencyStamp,
  });

  final String decision;
  final String concurrencyStamp;

  factory ComplaintCloseInputDto.fromJson(Map<String, dynamic> json) => ComplaintCloseInputDto(
        decision: (json['decision']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'decision': decision,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// نتيجة انتقال الشكوى. لا تحتوي محتوى، لأن المستدعي قد يفقد حق الوصول بعد العملية.
class ComplaintTransitionResultDto {
  const ComplaintTransitionResultDto({
    required this.id,
    required this.level,
    required this.status,
    this.closedAt,
    required this.callerRetainsAccess,
    required this.concurrencyStamp,
  });

  final String id;
  final ComplaintLevel level;
  final ComplaintStatus status;
  final DateTime? closedAt;
  /// false يعني أن الواجهة يجب أن تغادر شاشة التفاصيل وتحذف ما حفظته مؤقتًا.
  final bool callerRetainsAccess;
  final String concurrencyStamp;

  factory ComplaintTransitionResultDto.fromJson(Map<String, dynamic> json) => ComplaintTransitionResultDto(
        id: (json['id']) as String,
        level: ComplaintLevel.fromJson(json['level']),
        status: ComplaintStatus.fromJson(json['status']),
        closedAt: json['closedAt'] == null ? null : DateTime.parse((json['closedAt']) as String),
        callerRetainsAccess: (json['callerRetainsAccess']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'level': level.toJson(),
        'status': status.toJson(),
        'closedAt': closedAt == null ? null : hrDateTimeToJson(closedAt!),
        'callerRetainsAccess': callerRetainsAccess,
        'concurrencyStamp': concurrencyStamp,
      };
}

class ComplaintAttachmentUploadDto {
  const ComplaintAttachmentUploadDto({
    required this.file,
  });

  /// يُفحص من الفيروسات ويُشفر قبل التخزين.
  final MultipartFile file;

  Map<String, dynamic> toJson() => <String, dynamic>{
      };

  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,
  /// one part per file for arrays (x-server-requirements.multipart).
  FormData toFormData() {
    final form = FormData();
    form.files.add(MapEntry('file', file));
    return form;
  }
}

/// سجل إضافة فقط. لا يحتوي محتوى الشكوى.
class ComplaintAccessLogEntryDto {
  const ComplaintAccessLogEntryDto({
    required this.id,
    required this.user,
    required this.action,
    required this.at,
  });

  final String id;
  final UserRefDto user;
  final ComplaintAccessAction action;
  final DateTime at;

  factory ComplaintAccessLogEntryDto.fromJson(Map<String, dynamic> json) => ComplaintAccessLogEntryDto(
        id: (json['id']) as String,
        user: UserRefDto.fromJson((json['user']) as Map<String, dynamic>),
        action: ComplaintAccessAction.fromJson(json['action']),
        at: DateTime.parse((json['at']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'user': user.toJson(),
        'action': action.toJson(),
        'at': hrDateTimeToJson(at),
      };
}

class ComplaintAccessLogEntryPagedResultDto {
  const ComplaintAccessLogEntryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ComplaintAccessLogEntryDto> items;

  factory ComplaintAccessLogEntryPagedResultDto.fromJson(Map<String, dynamic> json) => ComplaintAccessLogEntryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ComplaintAccessLogEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class ComplaintHandlerAssignmentCreateDto {
  const ComplaintHandlerAssignmentCreateDto({
    required this.userId,
    required this.level,
    required this.from,
    this.to,
    required this.reason,
  });

  final String userId;
  final ComplaintLevel level;
  final DateTime from;
  /// null يعني دون تاريخ انتهاء، حتى السحب.
  final DateTime? to;
  final String reason;

  factory ComplaintHandlerAssignmentCreateDto.fromJson(Map<String, dynamic> json) => ComplaintHandlerAssignmentCreateDto(
        userId: (json['userId']) as String,
        level: ComplaintLevel.fromJson(json['level']),
        from: DateTime.parse((json['from']) as String),
        to: json['to'] == null ? null : DateTime.parse((json['to']) as String),
        reason: (json['reason']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'userId': userId,
        'level': level.toJson(),
        'from': hrDateTimeToJson(from),
        if (to != null) 'to': hrDateTimeToJson(to!),
        'reason': reason,
      };
}

/// لا يحتوي أي بيانات عن شكاوى.
class ComplaintHandlerAssignmentDto {
  const ComplaintHandlerAssignmentDto({
    required this.id,
    required this.user,
    required this.level,
    required this.status,
    required this.from,
    this.to,
    required this.reason,
    required this.requestedBy,
    required this.requestedAt,
    this.decidedBy,
    this.decidedAt,
    this.decisionReason,
    required this.concurrencyStamp,
  });

  final String id;
  final UserRefDto user;
  final ComplaintLevel level;
  final ComplaintHandlerAssignmentStatus status;
  final DateTime from;
  final DateTime? to;
  final String reason;
  final UserRefDto requestedBy;
  final DateTime requestedAt;
  final UserRefDto? decidedBy;
  final DateTime? decidedAt;
  final String? decisionReason;
  final String concurrencyStamp;

  factory ComplaintHandlerAssignmentDto.fromJson(Map<String, dynamic> json) => ComplaintHandlerAssignmentDto(
        id: (json['id']) as String,
        user: UserRefDto.fromJson((json['user']) as Map<String, dynamic>),
        level: ComplaintLevel.fromJson(json['level']),
        status: ComplaintHandlerAssignmentStatus.fromJson(json['status']),
        from: DateTime.parse((json['from']) as String),
        to: json['to'] == null ? null : DateTime.parse((json['to']) as String),
        reason: (json['reason']) as String,
        requestedBy: UserRefDto.fromJson((json['requestedBy']) as Map<String, dynamic>),
        requestedAt: DateTime.parse((json['requestedAt']) as String),
        decidedBy: json['decidedBy'] == null ? null : UserRefDto.fromJson((json['decidedBy']) as Map<String, dynamic>),
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse((json['decidedAt']) as String),
        decisionReason: json['decisionReason'] == null ? null : (json['decisionReason']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'user': user.toJson(),
        'level': level.toJson(),
        'status': status.toJson(),
        'from': hrDateTimeToJson(from),
        'to': to == null ? null : hrDateTimeToJson(to!),
        'reason': reason,
        'requestedBy': requestedBy.toJson(),
        'requestedAt': hrDateTimeToJson(requestedAt),
        'decidedBy': decidedBy == null ? null : decidedBy!.toJson(),
        'decidedAt': decidedAt == null ? null : hrDateTimeToJson(decidedAt!),
        'decisionReason': decisionReason == null ? null : decisionReason!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class ComplaintHandlerAssignmentPagedResultDto {
  const ComplaintHandlerAssignmentPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ComplaintHandlerAssignmentDto> items;

  factory ComplaintHandlerAssignmentPagedResultDto.fromJson(Map<String, dynamic> json) => ComplaintHandlerAssignmentPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ComplaintHandlerAssignmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class ComplaintHandlerAssignmentApproveDto {
  const ComplaintHandlerAssignmentApproveDto({
    required this.concurrencyStamp,
  });

  final String concurrencyStamp;

  factory ComplaintHandlerAssignmentApproveDto.fromJson(Map<String, dynamic> json) => ComplaintHandlerAssignmentApproveDto(
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'concurrencyStamp': concurrencyStamp,
      };
}

class ComplaintHandlerAssignmentReasonDto {
  const ComplaintHandlerAssignmentReasonDto({
    required this.reason,
    required this.concurrencyStamp,
  });

  final String reason;
  final String concurrencyStamp;

  factory ComplaintHandlerAssignmentReasonDto.fromJson(Map<String, dynamic> json) => ComplaintHandlerAssignmentReasonDto(
        reason: (json['reason']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'reason': reason,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// scheduled قبل الفتح، open بعد الفتح، closed عند فتح الدورة التالية.
enum EvaluationCycleStatus {
  scheduled('scheduled'),
  open('open'),
  closed('closed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const EvaluationCycleStatus(this.value);
  final String value;

  static EvaluationCycleStatus fromJson(Object? json) {
    for (final v in EvaluationCycleStatus.values) {
      if (v.value == json) return v;
    }
    return EvaluationCycleStatus.$unknown;
  }

  String toJson() => value;
}

/// حالة التقييم.
enum EvaluationStatus {
  draft('draft'),
  submitted('submitted'),
  acknowledged('acknowledged'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const EvaluationStatus(this.value);
  final String value;

  static EvaluationStatus fromJson(Object? json) {
    for (final v in EvaluationStatus.values) {
      if (v.value == json) return v;
    }
    return EvaluationStatus.$unknown;
  }

  String toJson() => value;
}

/// حالة آخر اعتراض.
enum EvaluationObjectionStatus {
  none('none'),
  pending('pending'),
  accepted('accepted'),
  rejected('rejected'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const EvaluationObjectionStatus(this.value);
  final String value;

  static EvaluationObjectionStatus fromJson(Object? json) {
    for (final v in EvaluationObjectionStatus.values) {
      if (v.value == json) return v;
    }
    return EvaluationObjectionStatus.$unknown;
  }

  String toJson() => value;
}

/// تعليق أو اعتراض من الموظف.
enum EvaluationResponseType {
  comment('comment'),
  objection('objection'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const EvaluationResponseType(this.value);
  final String value;

  static EvaluationResponseType fromJson(Object? json) {
    for (final v in EvaluationResponseType.values) {
      if (v.value == json) return v;
    }
    return EvaluationResponseType.$unknown;
  }

  String toJson() => value;
}

/// قرار مدير الموارد البشرية في الاعتراض.
enum EvaluationObjectionDecision {
  accepted('accepted'),
  rejected('rejected'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const EvaluationObjectionDecision(this.value);
  final String value;

  static EvaluationObjectionDecision fromJson(Object? json) {
    for (final v in EvaluationObjectionDecision.values) {
      if (v.value == json) return v;
    }
    return EvaluationObjectionDecision.$unknown;
  }

  String toJson() => value;
}

/// حالة التقرير الشهري.
enum DepartmentReportStatus {
  draft('draft'),
  submitted('submitted'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const DepartmentReportStatus(this.value);
  final String value;

  static DepartmentReportStatus fromJson(Object? json) {
    for (final v in DepartmentReportStatus.values) {
      if (v.value == json) return v;
    }
    return DepartmentReportStatus.$unknown;
  }

  String toJson() => value;
}

/// إنجاز المدير المستدعي في الدورة، وهو أساس التنبيه الموحد.
class EvaluationManagerProgressDto {
  const EvaluationManagerProgressDto({
    required this.expectedCount,
    required this.submittedCount,
    required this.remainingCount,
    this.reportId,
    this.reportStatus,
    required this.isCompleted,
    required this.isLate,
  });

  final int expectedCount;
  final int submittedCount;
  final int remainingCount;
  /// null قبل توليد التقرير في أول يوم دوام من الشهر التالي.
  final String? reportId;
  final DepartmentReportStatus? reportStatus;
  /// true فقط إذا رُفعت كل التقييمات والتقرير معًا.
  final bool isCompleted;
  final bool isLate;

  factory EvaluationManagerProgressDto.fromJson(Map<String, dynamic> json) => EvaluationManagerProgressDto(
        expectedCount: ((json['expectedCount']) as num).toInt(),
        submittedCount: ((json['submittedCount']) as num).toInt(),
        remainingCount: ((json['remainingCount']) as num).toInt(),
        reportId: json['reportId'] == null ? null : (json['reportId']) as String,
        reportStatus: json['reportStatus'] == null ? null : DepartmentReportStatus.fromJson(json['reportStatus']),
        isCompleted: (json['isCompleted']) as bool,
        isLate: (json['isLate']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'expectedCount': expectedCount,
        'submittedCount': submittedCount,
        'remainingCount': remainingCount,
        'reportId': reportId == null ? null : reportId!,
        'reportStatus': reportStatus == null ? null : reportStatus!.toJson(),
        'isCompleted': isCompleted,
        'isLate': isLate,
      };
}

class EvaluationCycleDto {
  const EvaluationCycleDto({
    required this.id,
    required this.year,
    required this.month,
    required this.status,
    required this.opensAt,
    required this.dataFreezeAt,
    required this.dueAt,
    required this.escalatesAt,
    this.managerProgress,
  });

  final String id;
  final int year;
  final int month;
  final EvaluationCycleStatus status;
  /// آخر يوم دوام من الشهر 10:00 افتراضيًا.
  final DateTime opensAt;
  /// أول يوم دوام من الشهر التالي.
  final DateTime dataFreezeAt;
  /// نهاية يوم الدوام الثالث من الشهر التالي افتراضيًا.
  final DateTime dueAt;
  /// يوم الدوام الرابع افتراضيًا.
  final DateTime escalatesAt;
  /// null إذا لم يكن المستدعي مدير قسم له موظفون.
  final EvaluationManagerProgressDto? managerProgress;

  factory EvaluationCycleDto.fromJson(Map<String, dynamic> json) => EvaluationCycleDto(
        id: (json['id']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        status: EvaluationCycleStatus.fromJson(json['status']),
        opensAt: DateTime.parse((json['opensAt']) as String),
        dataFreezeAt: DateTime.parse((json['dataFreezeAt']) as String),
        dueAt: DateTime.parse((json['dueAt']) as String),
        escalatesAt: DateTime.parse((json['escalatesAt']) as String),
        managerProgress: json['managerProgress'] == null ? null : EvaluationManagerProgressDto.fromJson((json['managerProgress']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'year': year,
        'month': month,
        'status': status.toJson(),
        'opensAt': hrDateTimeToJson(opensAt),
        'dataFreezeAt': hrDateTimeToJson(dataFreezeAt),
        'dueAt': hrDateTimeToJson(dueAt),
        'escalatesAt': hrDateTimeToJson(escalatesAt),
        'managerProgress': managerProgress == null ? null : managerProgress!.toJson(),
      };
}

/// بيانات حضور الموظف في شهر الدورة.
class EvaluationAttendanceSummaryDto {
  const EvaluationAttendanceSummaryDto({
    required this.periodFrom,
    required this.periodTo,
    required this.scheduledWorkingDays,
    required this.presentDays,
    required this.absentDays,
    required this.lateCount,
    required this.lateMinutes,
    required this.leaveDays,
    required this.isFrozen,
    this.frozenAt,
  });

  final DateTime periodFrom;
  final DateTime periodTo;
  final int scheduledWorkingDays;
  final int presentDays;
  final int absentDays;
  final int lateCount;
  final int lateMinutes;
  final double leaveDays;
  /// false قبل تثبيت بيانات الشهر، وقد تتغير القيم حتى التثبيت.
  final bool isFrozen;
  final DateTime? frozenAt;

  factory EvaluationAttendanceSummaryDto.fromJson(Map<String, dynamic> json) => EvaluationAttendanceSummaryDto(
        periodFrom: hrDateFromJson((json['periodFrom']) as String),
        periodTo: hrDateFromJson((json['periodTo']) as String),
        scheduledWorkingDays: ((json['scheduledWorkingDays']) as num).toInt(),
        presentDays: ((json['presentDays']) as num).toInt(),
        absentDays: ((json['absentDays']) as num).toInt(),
        lateCount: ((json['lateCount']) as num).toInt(),
        lateMinutes: ((json['lateMinutes']) as num).toInt(),
        leaveDays: ((json['leaveDays']) as num).toDouble(),
        isFrozen: (json['isFrozen']) as bool,
        frozenAt: json['frozenAt'] == null ? null : DateTime.parse((json['frozenAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'periodFrom': hrDateToJson(periodFrom),
        'periodTo': hrDateToJson(periodTo),
        'scheduledWorkingDays': scheduledWorkingDays,
        'presentDays': presentDays,
        'absentDays': absentDays,
        'lateCount': lateCount,
        'lateMinutes': lateMinutes,
        'leaveDays': leaveDays,
        'isFrozen': isFrozen,
        'frozenAt': frozenAt == null ? null : hrDateTimeToJson(frozenAt!),
      };
}

/// خيار في السلم اللفظي. لا يحتوي قيمة رقمية أو نسبة.
class EvaluationGradeOptionDto {
  const EvaluationGradeOptionDto({
    required this.code,
    required this.name,
  });

  final String code;
  /// مترجم حسب Accept-Language.
  final String name;

  factory EvaluationGradeOptionDto.fromJson(Map<String, dynamic> json) => EvaluationGradeOptionDto(
        code: (json['code']) as String,
        name: (json['name']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'code': code,
        'name': name,
      };
}

/// درجة معيار في نموذج المدير. القيم null تعني أن المعيار لم يُقيَّم بعد.
class EvaluationDraftScoreDto {
  const EvaluationDraftScoreDto({
    required this.criterionId,
    required this.criterionName,
    this.gradeCode,
    this.gradeName,
  });

  final String criterionId;
  final String criterionName;
  final String? gradeCode;
  final String? gradeName;

  factory EvaluationDraftScoreDto.fromJson(Map<String, dynamic> json) => EvaluationDraftScoreDto(
        criterionId: (json['criterionId']) as String,
        criterionName: (json['criterionName']) as String,
        gradeCode: json['gradeCode'] == null ? null : (json['gradeCode']) as String,
        gradeName: json['gradeName'] == null ? null : (json['gradeName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'criterionId': criterionId,
        'criterionName': criterionName,
        'gradeCode': gradeCode == null ? null : gradeCode!,
        'gradeName': gradeName == null ? null : gradeName!,
      };
}

/// درجة معيار بعد الرفع، بالسلم اللفظي فقط.
class EvaluationSubmittedScoreDto {
  const EvaluationSubmittedScoreDto({
    required this.criterionId,
    required this.criterionName,
    required this.gradeCode,
    required this.gradeName,
  });

  final String criterionId;
  final String criterionName;
  final String gradeCode;
  final String gradeName;

  factory EvaluationSubmittedScoreDto.fromJson(Map<String, dynamic> json) => EvaluationSubmittedScoreDto(
        criterionId: (json['criterionId']) as String,
        criterionName: (json['criterionName']) as String,
        gradeCode: (json['gradeCode']) as String,
        gradeName: (json['gradeName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'criterionId': criterionId,
        'criterionName': criterionName,
        'gradeCode': gradeCode,
        'gradeName': gradeName,
      };
}

class EvaluationResponseDto {
  const EvaluationResponseDto({
    required this.id,
    required this.type,
    required this.content,
    required this.creationTime,
    this.decision,
    this.decisionReason,
    this.decidedBy,
    this.decidedAt,
  });

  final String id;
  final EvaluationResponseType type;
  final String content;
  final DateTime creationTime;
  final EvaluationObjectionDecision? decision;
  final String? decisionReason;
  final UserRefDto? decidedBy;
  final DateTime? decidedAt;

  factory EvaluationResponseDto.fromJson(Map<String, dynamic> json) => EvaluationResponseDto(
        id: (json['id']) as String,
        type: EvaluationResponseType.fromJson(json['type']),
        content: (json['content']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
        decision: json['decision'] == null ? null : EvaluationObjectionDecision.fromJson(json['decision']),
        decisionReason: json['decisionReason'] == null ? null : (json['decisionReason']) as String,
        decidedBy: json['decidedBy'] == null ? null : UserRefDto.fromJson((json['decidedBy']) as Map<String, dynamic>),
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse((json['decidedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'type': type.toJson(),
        'content': content,
        'creationTime': hrDateTimeToJson(creationTime),
        'decision': decision == null ? null : decision!.toJson(),
        'decisionReason': decisionReason == null ? null : decisionReason!,
        'decidedBy': decidedBy == null ? null : decidedBy!.toJson(),
        'decidedAt': decidedAt == null ? null : hrDateTimeToJson(decidedAt!),
      };
}

/// عرض المدير: النموذج مع بيانات الحضور. الملاحظة الداخلية غير موجودة في هذا الـDTO إطلاقًا، وتُدار فقط عبر مسارات internal-note المنفصلة.
class EvaluationManagerDto {
  const EvaluationManagerDto({
    required this.id,
    required this.cycleId,
    required this.year,
    required this.month,
    required this.employee,
    required this.status,
    required this.isEditable,
    required this.attendance,
    required this.availableGrades,
    required this.scores,
    this.employeeNotes,
    this.submittedAt,
    this.acknowledgedAt,
    required this.objectionStatus,
    required this.responses,
    required this.concurrencyStamp,
  });

  final String id;
  final String cycleId;
  final int year;
  final int month;
  final EmployeeRefDto employee;
  final EvaluationStatus status;
  /// true فقط للحالة draft عندما تكون الدورة مفتوحة والمستدعي هو المقيِّم.
  final bool isEditable;
  final EvaluationAttendanceSummaryDto attendance;
  /// بترتيب العرض.
  final List<EvaluationGradeOptionDto> availableGrades;
  /// عنصر لكل معيار مثبت في الدورة.
  final List<EvaluationDraftScoreDto> scores;
  /// ملاحظات للموظف، يراها الموظف ومديره والموارد البشرية.
  final String? employeeNotes;
  final DateTime? submittedAt;
  final DateTime? acknowledgedAt;
  final EvaluationObjectionStatus objectionStatus;
  final List<EvaluationResponseDto> responses;
  final String concurrencyStamp;

  factory EvaluationManagerDto.fromJson(Map<String, dynamic> json) => EvaluationManagerDto(
        id: (json['id']) as String,
        cycleId: (json['cycleId']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        status: EvaluationStatus.fromJson(json['status']),
        isEditable: (json['isEditable']) as bool,
        attendance: EvaluationAttendanceSummaryDto.fromJson((json['attendance']) as Map<String, dynamic>),
        availableGrades: ((json['availableGrades']) as List).map((e) => EvaluationGradeOptionDto.fromJson((e) as Map<String, dynamic>)).toList(),
        scores: ((json['scores']) as List).map((e) => EvaluationDraftScoreDto.fromJson((e) as Map<String, dynamic>)).toList(),
        employeeNotes: json['employeeNotes'] == null ? null : (json['employeeNotes']) as String,
        submittedAt: json['submittedAt'] == null ? null : DateTime.parse((json['submittedAt']) as String),
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
        objectionStatus: EvaluationObjectionStatus.fromJson(json['objectionStatus']),
        responses: ((json['responses']) as List).map((e) => EvaluationResponseDto.fromJson((e) as Map<String, dynamic>)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'cycleId': cycleId,
        'year': year,
        'month': month,
        'employee': employee.toJson(),
        'status': status.toJson(),
        'isEditable': isEditable,
        'attendance': attendance.toJson(),
        'availableGrades': availableGrades.map((e) => e.toJson()).toList(),
        'scores': scores.map((e) => e.toJson()).toList(),
        'employeeNotes': employeeNotes == null ? null : employeeNotes!,
        'submittedAt': submittedAt == null ? null : hrDateTimeToJson(submittedAt!),
        'acknowledgedAt': acknowledgedAt == null ? null : hrDateTimeToJson(acknowledgedAt!),
        'objectionStatus': objectionStatus.toJson(),
        'responses': responses.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
      };
}

/// عنصر في قائمة المدير. الملاحظة الداخلية غير موجودة في هذا الـDTO إطلاقًا، وتُدار فقط عبر مسارات internal-note المنفصلة.
class EvaluationManagerSummaryDto {
  const EvaluationManagerSummaryDto({
    required this.id,
    required this.cycleId,
    required this.year,
    required this.month,
    required this.employee,
    required this.status,
    required this.objectionStatus,
    this.submittedAt,
  });

  final String id;
  final String cycleId;
  final int year;
  final int month;
  final EmployeeRefDto employee;
  final EvaluationStatus status;
  final EvaluationObjectionStatus objectionStatus;
  final DateTime? submittedAt;

  factory EvaluationManagerSummaryDto.fromJson(Map<String, dynamic> json) => EvaluationManagerSummaryDto(
        id: (json['id']) as String,
        cycleId: (json['cycleId']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        status: EvaluationStatus.fromJson(json['status']),
        objectionStatus: EvaluationObjectionStatus.fromJson(json['objectionStatus']),
        submittedAt: json['submittedAt'] == null ? null : DateTime.parse((json['submittedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'cycleId': cycleId,
        'year': year,
        'month': month,
        'employee': employee.toJson(),
        'status': status.toJson(),
        'objectionStatus': objectionStatus.toJson(),
        'submittedAt': submittedAt == null ? null : hrDateTimeToJson(submittedAt!),
      };
}

class EvaluationManagerSummaryPagedResultDto {
  const EvaluationManagerSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EvaluationManagerSummaryDto> items;

  factory EvaluationManagerSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => EvaluationManagerSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EvaluationManagerSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EvaluationScoreInputDto {
  const EvaluationScoreInputDto({
    required this.criterionId,
    required this.gradeCode,
  });

  final String criterionId;
  /// رمز من availableGrades.
  final String gradeCode;

  factory EvaluationScoreInputDto.fromJson(Map<String, dynamic> json) => EvaluationScoreInputDto(
        criterionId: (json['criterionId']) as String,
        gradeCode: (json['gradeCode']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'criterionId': criterionId,
        'gradeCode': gradeCode,
      };
}

/// لا يحتوي حقلًا للملاحظة الداخلية. أي حقل إضافي يُرفض.
class EvaluationSaveDraftDto {
  const EvaluationSaveDraftDto({
    required this.scores,
    this.employeeNotes,
    required this.concurrencyStamp,
  });

  /// يستبدل الدرجات المحفوظة. يمكن أن يكون جزئيًا في المسودة.
  final List<EvaluationScoreInputDto> scores;
  final String? employeeNotes;
  final String concurrencyStamp;

  factory EvaluationSaveDraftDto.fromJson(Map<String, dynamic> json) => EvaluationSaveDraftDto(
        scores: ((json['scores']) as List).map((e) => EvaluationScoreInputDto.fromJson((e) as Map<String, dynamic>)).toList(),
        employeeNotes: json['employeeNotes'] == null ? null : (json['employeeNotes']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'scores': scores.map((e) => e.toJson()).toList(),
        if (employeeNotes != null) 'employeeNotes': employeeNotes!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class EvaluationSubmitInputDto {
  const EvaluationSubmitInputDto({
    required this.concurrencyStamp,
  });

  final String concurrencyStamp;

  factory EvaluationSubmitInputDto.fromJson(Map<String, dynamic> json) => EvaluationSubmitInputDto(
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'concurrencyStamp': concurrencyStamp,
      };
}

/// عرض الموظف. لا يحتوي الملاحظة الداخلية ولا أي مؤشر على وجودها. الحقل غائب وليس null.
class EvaluationEmployeeDto {
  const EvaluationEmployeeDto({
    required this.id,
    required this.year,
    required this.month,
    required this.evaluator,
    required this.status,
    required this.scores,
    this.employeeNotes,
    required this.submittedAt,
    this.acknowledgedAt,
    required this.objectionStatus,
    required this.responses,
  });

  final String id;
  final int year;
  final int month;
  final UserRefDto evaluator;
  final EvaluationStatus status;
  final List<EvaluationSubmittedScoreDto> scores;
  final String? employeeNotes;
  final DateTime submittedAt;
  final DateTime? acknowledgedAt;
  final EvaluationObjectionStatus objectionStatus;
  final List<EvaluationResponseDto> responses;

  factory EvaluationEmployeeDto.fromJson(Map<String, dynamic> json) => EvaluationEmployeeDto(
        id: (json['id']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        evaluator: UserRefDto.fromJson((json['evaluator']) as Map<String, dynamic>),
        status: EvaluationStatus.fromJson(json['status']),
        scores: ((json['scores']) as List).map((e) => EvaluationSubmittedScoreDto.fromJson((e) as Map<String, dynamic>)).toList(),
        employeeNotes: json['employeeNotes'] == null ? null : (json['employeeNotes']) as String,
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
        objectionStatus: EvaluationObjectionStatus.fromJson(json['objectionStatus']),
        responses: ((json['responses']) as List).map((e) => EvaluationResponseDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'year': year,
        'month': month,
        'evaluator': evaluator.toJson(),
        'status': status.toJson(),
        'scores': scores.map((e) => e.toJson()).toList(),
        'employeeNotes': employeeNotes == null ? null : employeeNotes!,
        'submittedAt': hrDateTimeToJson(submittedAt),
        'acknowledgedAt': acknowledgedAt == null ? null : hrDateTimeToJson(acknowledgedAt!),
        'objectionStatus': objectionStatus.toJson(),
        'responses': responses.map((e) => e.toJson()).toList(),
      };
}

/// عنصر في تقييماتي.
class EvaluationEmployeeSummaryDto {
  const EvaluationEmployeeSummaryDto({
    required this.id,
    required this.year,
    required this.month,
    required this.status,
    required this.objectionStatus,
    required this.submittedAt,
    this.acknowledgedAt,
  });

  final String id;
  final int year;
  final int month;
  final EvaluationStatus status;
  final EvaluationObjectionStatus objectionStatus;
  final DateTime submittedAt;
  final DateTime? acknowledgedAt;

  factory EvaluationEmployeeSummaryDto.fromJson(Map<String, dynamic> json) => EvaluationEmployeeSummaryDto(
        id: (json['id']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        status: EvaluationStatus.fromJson(json['status']),
        objectionStatus: EvaluationObjectionStatus.fromJson(json['objectionStatus']),
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'year': year,
        'month': month,
        'status': status.toJson(),
        'objectionStatus': objectionStatus.toJson(),
        'submittedAt': hrDateTimeToJson(submittedAt),
        'acknowledgedAt': acknowledgedAt == null ? null : hrDateTimeToJson(acknowledgedAt!),
      };
}

class EvaluationEmployeeSummaryPagedResultDto {
  const EvaluationEmployeeSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EvaluationEmployeeSummaryDto> items;

  factory EvaluationEmployeeSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => EvaluationEmployeeSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EvaluationEmployeeSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// عرض مدير الموارد البشرية. الملاحظة الداخلية غير موجودة في هذا الـDTO إطلاقًا، وتُدار فقط عبر مسارات internal-note المنفصلة.
class EvaluationHrDto {
  const EvaluationHrDto({
    required this.id,
    required this.cycleId,
    required this.year,
    required this.month,
    required this.employee,
    required this.evaluator,
    required this.status,
    required this.attendance,
    required this.scores,
    this.employeeNotes,
    required this.submittedAt,
    this.acknowledgedAt,
    required this.objectionStatus,
    required this.responses,
    required this.concurrencyStamp,
  });

  final String id;
  final String cycleId;
  final int year;
  final int month;
  final EmployeeRefDto employee;
  final UserRefDto evaluator;
  final EvaluationStatus status;
  final EvaluationAttendanceSummaryDto attendance;
  final List<EvaluationSubmittedScoreDto> scores;
  final String? employeeNotes;
  final DateTime submittedAt;
  final DateTime? acknowledgedAt;
  final EvaluationObjectionStatus objectionStatus;
  final List<EvaluationResponseDto> responses;
  final String concurrencyStamp;

  factory EvaluationHrDto.fromJson(Map<String, dynamic> json) => EvaluationHrDto(
        id: (json['id']) as String,
        cycleId: (json['cycleId']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        evaluator: UserRefDto.fromJson((json['evaluator']) as Map<String, dynamic>),
        status: EvaluationStatus.fromJson(json['status']),
        attendance: EvaluationAttendanceSummaryDto.fromJson((json['attendance']) as Map<String, dynamic>),
        scores: ((json['scores']) as List).map((e) => EvaluationSubmittedScoreDto.fromJson((e) as Map<String, dynamic>)).toList(),
        employeeNotes: json['employeeNotes'] == null ? null : (json['employeeNotes']) as String,
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
        objectionStatus: EvaluationObjectionStatus.fromJson(json['objectionStatus']),
        responses: ((json['responses']) as List).map((e) => EvaluationResponseDto.fromJson((e) as Map<String, dynamic>)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'cycleId': cycleId,
        'year': year,
        'month': month,
        'employee': employee.toJson(),
        'evaluator': evaluator.toJson(),
        'status': status.toJson(),
        'attendance': attendance.toJson(),
        'scores': scores.map((e) => e.toJson()).toList(),
        'employeeNotes': employeeNotes == null ? null : employeeNotes!,
        'submittedAt': hrDateTimeToJson(submittedAt),
        'acknowledgedAt': acknowledgedAt == null ? null : hrDateTimeToJson(acknowledgedAt!),
        'objectionStatus': objectionStatus.toJson(),
        'responses': responses.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
      };
}

/// عنصر في قائمة الموارد البشرية. لا يحتوي درجات لأن المسودة ليست للعرض.
class EvaluationHrSummaryDto {
  const EvaluationHrSummaryDto({
    required this.id,
    required this.cycleId,
    required this.year,
    required this.month,
    required this.employee,
    required this.evaluator,
    required this.status,
    required this.objectionStatus,
    this.submittedAt,
    required this.isLate,
  });

  final String id;
  final String cycleId;
  final int year;
  final int month;
  final EmployeeRefDto employee;
  final UserRefDto evaluator;
  final EvaluationStatus status;
  final EvaluationObjectionStatus objectionStatus;
  final DateTime? submittedAt;
  final bool isLate;

  factory EvaluationHrSummaryDto.fromJson(Map<String, dynamic> json) => EvaluationHrSummaryDto(
        id: (json['id']) as String,
        cycleId: (json['cycleId']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        evaluator: UserRefDto.fromJson((json['evaluator']) as Map<String, dynamic>),
        status: EvaluationStatus.fromJson(json['status']),
        objectionStatus: EvaluationObjectionStatus.fromJson(json['objectionStatus']),
        submittedAt: json['submittedAt'] == null ? null : DateTime.parse((json['submittedAt']) as String),
        isLate: (json['isLate']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'cycleId': cycleId,
        'year': year,
        'month': month,
        'employee': employee.toJson(),
        'evaluator': evaluator.toJson(),
        'status': status.toJson(),
        'objectionStatus': objectionStatus.toJson(),
        'submittedAt': submittedAt == null ? null : hrDateTimeToJson(submittedAt!),
        'isLate': isLate,
      };
}

class EvaluationHrSummaryPagedResultDto {
  const EvaluationHrSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EvaluationHrSummaryDto> items;

  factory EvaluationHrSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => EvaluationHrSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EvaluationHrSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EvaluationResponseCreateDto {
  const EvaluationResponseCreateDto({
    required this.type,
    required this.content,
  });

  final EvaluationResponseType type;
  final String content;

  factory EvaluationResponseCreateDto.fromJson(Map<String, dynamic> json) => EvaluationResponseCreateDto(
        type: EvaluationResponseType.fromJson(json['type']),
        content: (json['content']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'type': type.toJson(),
        'content': content,
      };
}

/// السبب يصل إلى الموظف، ويجب أن يستند إلى الدرجات والملاحظات الظاهرة والوقائع الموثقة، ولا يذكر أي ملاحظة داخلية.
class EvaluationObjectionDecideDto {
  const EvaluationObjectionDecideDto({
    this.scores,
    required this.decision,
    required this.reason,
    required this.concurrencyStamp,
  });

  /// With decision = accepted: the corrected grades. Criteria not listed keep their grade.
  final List<EvaluationScoreInputDto>? scores;
  final EvaluationObjectionDecision decision;
  final String reason;
  final String concurrencyStamp;

  factory EvaluationObjectionDecideDto.fromJson(Map<String, dynamic> json) => EvaluationObjectionDecideDto(
        scores: json['scores'] == null ? null : ((json['scores']) as List).map((e) => EvaluationScoreInputDto.fromJson((e) as Map<String, dynamic>)).toList(),
        decision: EvaluationObjectionDecision.fromJson(json['decision']),
        reason: (json['reason']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (scores != null) 'scores': scores!.map((e) => e.toJson()).toList(),
        'decision': decision.toJson(),
        'reason': reason,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// ملاحظة داخلية. يراها كاتبها ومدير الموارد البشرية فقط. لا تُضمَّن في أي DTO آخر.
class EvaluationInternalNoteDto {
  const EvaluationInternalNoteDto({
    required this.id,
    required this.author,
    required this.content,
    required this.isDraft,
    required this.creationTime,
  });

  final String id;
  final UserRefDto author;
  final String content;
  /// true لمسودة كاتبها قبل رفع التقييم، ولا تظهر إلا له.
  final bool isDraft;
  final DateTime creationTime;

  factory EvaluationInternalNoteDto.fromJson(Map<String, dynamic> json) => EvaluationInternalNoteDto(
        id: (json['id']) as String,
        author: UserRefDto.fromJson((json['author']) as Map<String, dynamic>),
        content: (json['content']) as String,
        isDraft: (json['isDraft']) as bool,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'author': author.toJson(),
        'content': content,
        'isDraft': isDraft,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class EvaluationInternalNotePagedResultDto {
  const EvaluationInternalNotePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EvaluationInternalNoteDto> items;

  factory EvaluationInternalNotePagedResultDto.fromJson(Map<String, dynamic> json) => EvaluationInternalNotePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EvaluationInternalNoteDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EvaluationInternalNoteCreateDto {
  const EvaluationInternalNoteCreateDto({
    required this.content,
  });

  final String content;

  factory EvaluationInternalNoteCreateDto.fromJson(Map<String, dynamic> json) => EvaluationInternalNoteCreateDto(
        content: (json['content']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'content': content,
      };
}

class EvaluationInternalNoteDraftDto {
  const EvaluationInternalNoteDraftDto({
    this.content,
  });

  /// null أو نص فارغ يحذف المسودة.
  final String? content;

  factory EvaluationInternalNoteDraftDto.fromJson(Map<String, dynamic> json) => EvaluationInternalNoteDraftDto(
        content: json['content'] == null ? null : (json['content']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'content': content == null ? null : content!,
      };
}

/// عدد مرات الدرجة، دون نسب.
class EvaluationGradeCountDto {
  const EvaluationGradeCountDto({
    required this.gradeCode,
    required this.gradeName,
    required this.count,
  });

  final String gradeCode;
  final String gradeName;
  final int count;

  factory EvaluationGradeCountDto.fromJson(Map<String, dynamic> json) => EvaluationGradeCountDto(
        gradeCode: (json['gradeCode']) as String,
        gradeName: (json['gradeName']) as String,
        count: ((json['count']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'gradeCode': gradeCode,
        'gradeName': gradeName,
        'count': count,
      };
}

class EvaluationAnnualCriterionSummaryDto {
  const EvaluationAnnualCriterionSummaryDto({
    required this.criterionId,
    required this.criterionName,
    required this.gradeCounts,
  });

  final String criterionId;
  final String criterionName;
  final List<EvaluationGradeCountDto> gradeCounts;

  factory EvaluationAnnualCriterionSummaryDto.fromJson(Map<String, dynamic> json) => EvaluationAnnualCriterionSummaryDto(
        criterionId: (json['criterionId']) as String,
        criterionName: (json['criterionName']) as String,
        gradeCounts: ((json['gradeCounts']) as List).map((e) => EvaluationGradeCountDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'criterionId': criterionId,
        'criterionName': criterionName,
        'gradeCounts': gradeCounts.map((e) => e.toJson()).toList(),
      };
}

/// ملخص سنوي يُحسب آليًا من التقييمات المرفوعة، بدل نموذج سنوي منفصل. لا يحتوي نسبًا ولا ملاحظات داخلية.
class EvaluationAnnualSummaryDto {
  const EvaluationAnnualSummaryDto({
    required this.employee,
    required this.year,
    required this.evaluatedMonthsCount,
    required this.acknowledgedCount,
    required this.objectionsCount,
    required this.criteria,
  });

  final EmployeeRefDto employee;
  final int year;
  final int evaluatedMonthsCount;
  final int acknowledgedCount;
  final int objectionsCount;
  final List<EvaluationAnnualCriterionSummaryDto> criteria;

  factory EvaluationAnnualSummaryDto.fromJson(Map<String, dynamic> json) => EvaluationAnnualSummaryDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        year: ((json['year']) as num).toInt(),
        evaluatedMonthsCount: ((json['evaluatedMonthsCount']) as num).toInt(),
        acknowledgedCount: ((json['acknowledgedCount']) as num).toInt(),
        objectionsCount: ((json['objectionsCount']) as num).toInt(),
        criteria: ((json['criteria']) as List).map((e) => EvaluationAnnualCriterionSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'year': year,
        'evaluatedMonthsCount': evaluatedMonthsCount,
        'acknowledgedCount': acknowledgedCount,
        'objectionsCount': objectionsCount,
        'criteria': criteria.map((e) => e.toJson()).toList(),
      };
}

class EvaluationCriterionDto {
  const EvaluationCriterionDto({
    required this.id,
    required this.name,
    required this.nameAr,
    required this.nameEn,
    required this.sortOrder,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String id;
  /// مترجم حسب Accept-Language.
  final String name;
  final String nameAr;
  final String nameEn;
  final int sortOrder;
  final bool isActive;
  final String concurrencyStamp;

  factory EvaluationCriterionDto.fromJson(Map<String, dynamic> json) => EvaluationCriterionDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        sortOrder: ((json['sortOrder']) as num).toInt(),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'sortOrder': sortOrder,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

class EvaluationCriterionPagedResultDto {
  const EvaluationCriterionPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EvaluationCriterionDto> items;

  factory EvaluationCriterionPagedResultDto.fromJson(Map<String, dynamic> json) => EvaluationCriterionPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EvaluationCriterionDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EvaluationCriterionCreateDto {
  const EvaluationCriterionCreateDto({
    required this.nameAr,
    required this.nameEn,
    required this.sortOrder,
    required this.isActive,
  });

  final String nameAr;
  final String nameEn;
  final int sortOrder;
  final bool isActive;

  factory EvaluationCriterionCreateDto.fromJson(Map<String, dynamic> json) => EvaluationCriterionCreateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        sortOrder: ((json['sortOrder']) as num).toInt(),
        isActive: (json['isActive']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'sortOrder': sortOrder,
        'isActive': isActive,
      };
}

class EvaluationCriterionUpdateDto {
  const EvaluationCriterionUpdateDto({
    required this.nameAr,
    required this.nameEn,
    required this.sortOrder,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String nameAr;
  final String nameEn;
  final int sortOrder;
  final bool isActive;
  final String concurrencyStamp;

  factory EvaluationCriterionUpdateDto.fromJson(Map<String, dynamic> json) => EvaluationCriterionUpdateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        sortOrder: ((json['sortOrder']) as num).toInt(),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'sortOrder': sortOrder,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// عنصر في السلم اللفظي. sortOrder للعرض فقط، وليس قيمة أو وزنًا.
class EvaluationGradeDto {
  const EvaluationGradeDto({
    required this.id,
    required this.code,
    required this.name,
    required this.nameAr,
    required this.nameEn,
    required this.sortOrder,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String id;
  /// ثابت بعد الإنشاء.
  final String code;
  /// مترجم حسب Accept-Language.
  final String name;
  final String nameAr;
  final String nameEn;
  final int sortOrder;
  final bool isActive;
  final String concurrencyStamp;

  factory EvaluationGradeDto.fromJson(Map<String, dynamic> json) => EvaluationGradeDto(
        id: (json['id']) as String,
        code: (json['code']) as String,
        name: (json['name']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        sortOrder: ((json['sortOrder']) as num).toInt(),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'code': code,
        'name': name,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'sortOrder': sortOrder,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

class EvaluationGradePagedResultDto {
  const EvaluationGradePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EvaluationGradeDto> items;

  factory EvaluationGradePagedResultDto.fromJson(Map<String, dynamic> json) => EvaluationGradePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EvaluationGradeDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EvaluationGradeCreateDto {
  const EvaluationGradeCreateDto({
    required this.code,
    required this.nameAr,
    required this.nameEn,
    required this.sortOrder,
    required this.isActive,
  });

  final String code;
  final String nameAr;
  final String nameEn;
  final int sortOrder;
  final bool isActive;

  factory EvaluationGradeCreateDto.fromJson(Map<String, dynamic> json) => EvaluationGradeCreateDto(
        code: (json['code']) as String,
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        sortOrder: ((json['sortOrder']) as num).toInt(),
        isActive: (json['isActive']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'code': code,
        'nameAr': nameAr,
        'nameEn': nameEn,
        'sortOrder': sortOrder,
        'isActive': isActive,
      };
}

/// الرمز لا يتغير.
class EvaluationGradeUpdateDto {
  const EvaluationGradeUpdateDto({
    required this.nameAr,
    required this.nameEn,
    required this.sortOrder,
    required this.isActive,
    required this.concurrencyStamp,
  });

  final String nameAr;
  final String nameEn;
  final int sortOrder;
  final bool isActive;
  final String concurrencyStamp;

  factory EvaluationGradeUpdateDto.fromJson(Map<String, dynamic> json) => EvaluationGradeUpdateDto(
        nameAr: (json['nameAr']) as String,
        nameEn: (json['nameEn']) as String,
        sortOrder: ((json['sortOrder']) as num).toInt(),
        isActive: (json['isActive']) as bool,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'nameAr': nameAr,
        'nameEn': nameEn,
        'sortOrder': sortOrder,
        'isActive': isActive,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// إنجاز المدير في الدورة. لا يكون isCompleted صحيحًا إلا برفع كل التقييمات والتقرير معًا.
class DepartmentCompletionDto {
  const DepartmentCompletionDto({
    required this.evaluationsExpected,
    required this.evaluationsSubmitted,
    required this.reportSubmitted,
    required this.isCompleted,
    required this.isLate,
  });

  final int evaluationsExpected;
  final int evaluationsSubmitted;
  final bool reportSubmitted;
  final bool isCompleted;
  final bool isLate;

  factory DepartmentCompletionDto.fromJson(Map<String, dynamic> json) => DepartmentCompletionDto(
        evaluationsExpected: ((json['evaluationsExpected']) as num).toInt(),
        evaluationsSubmitted: ((json['evaluationsSubmitted']) as num).toInt(),
        reportSubmitted: (json['reportSubmitted']) as bool,
        isCompleted: (json['isCompleted']) as bool,
        isLate: (json['isLate']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'evaluationsExpected': evaluationsExpected,
        'evaluationsSubmitted': evaluationsSubmitted,
        'reportSubmitted': reportSubmitted,
        'isCompleted': isCompleted,
        'isLate': isLate,
      };
}

/// صف موظف في اللقطة. لا يحتوي درجات تفصيلية ولا ملاحظات داخلية ولا بيانات مالية.
class DepartmentReportEmployeeRowDto {
  const DepartmentReportEmployeeRowDto({
    required this.employee,
    required this.presentDays,
    required this.absentDays,
    required this.lateCount,
    required this.lateMinutes,
    required this.leaveDays,
    required this.pendingRequestsCount,
    this.evaluationStatus,
  });

  final EmployeeRefDto employee;
  final int presentDays;
  final int absentDays;
  final int lateCount;
  final int lateMinutes;
  final double leaveDays;
  final int pendingRequestsCount;
  /// null إذا لم يكن الموظف مشمولًا بالتقييم في هذه الدورة.
  final EvaluationStatus? evaluationStatus;

  factory DepartmentReportEmployeeRowDto.fromJson(Map<String, dynamic> json) => DepartmentReportEmployeeRowDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        presentDays: ((json['presentDays']) as num).toInt(),
        absentDays: ((json['absentDays']) as num).toInt(),
        lateCount: ((json['lateCount']) as num).toInt(),
        lateMinutes: ((json['lateMinutes']) as num).toInt(),
        leaveDays: ((json['leaveDays']) as num).toDouble(),
        pendingRequestsCount: ((json['pendingRequestsCount']) as num).toInt(),
        evaluationStatus: json['evaluationStatus'] == null ? null : EvaluationStatus.fromJson(json['evaluationStatus']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'presentDays': presentDays,
        'absentDays': absentDays,
        'lateCount': lateCount,
        'lateMinutes': lateMinutes,
        'leaveDays': leaveDays,
        'pendingRequestsCount': pendingRequestsCount,
        'evaluationStatus': evaluationStatus == null ? null : evaluationStatus!.toJson(),
      };
}

/// ملخص التقييمات. يستبعد الملاحظات الداخلية بالكامل.
class DepartmentReportEvaluationSummaryDto {
  const DepartmentReportEvaluationSummaryDto({
    required this.expectedCount,
    required this.submittedCount,
    required this.acknowledgedCount,
    required this.pendingObjectionsCount,
    required this.gradeCounts,
  });

  final int expectedCount;
  final int submittedCount;
  final int acknowledgedCount;
  final int pendingObjectionsCount;
  final List<EvaluationGradeCountDto> gradeCounts;

  factory DepartmentReportEvaluationSummaryDto.fromJson(Map<String, dynamic> json) => DepartmentReportEvaluationSummaryDto(
        expectedCount: ((json['expectedCount']) as num).toInt(),
        submittedCount: ((json['submittedCount']) as num).toInt(),
        acknowledgedCount: ((json['acknowledgedCount']) as num).toInt(),
        pendingObjectionsCount: ((json['pendingObjectionsCount']) as num).toInt(),
        gradeCounts: ((json['gradeCounts']) as List).map((e) => EvaluationGradeCountDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'expectedCount': expectedCount,
        'submittedCount': submittedCount,
        'acknowledgedCount': acknowledgedCount,
        'pendingObjectionsCount': pendingObjectionsCount,
        'gradeCounts': gradeCounts.map((e) => e.toJson()).toList(),
      };
}

/// عهدة القسم.
class DepartmentReportCustodySummaryDto {
  const DepartmentReportCustodySummaryDto({
    required this.sharedItemsCount,
    required this.personalItemsCount,
    required this.unacknowledgedItemsCount,
    this.lastCountDate,
  });

  final int sharedItemsCount;
  final int personalItemsCount;
  final int unacknowledgedItemsCount;
  final DateTime? lastCountDate;

  factory DepartmentReportCustodySummaryDto.fromJson(Map<String, dynamic> json) => DepartmentReportCustodySummaryDto(
        sharedItemsCount: ((json['sharedItemsCount']) as num).toInt(),
        personalItemsCount: ((json['personalItemsCount']) as num).toInt(),
        unacknowledgedItemsCount: ((json['unacknowledgedItemsCount']) as num).toInt(),
        lastCountDate: json['lastCountDate'] == null ? null : hrDateFromJson((json['lastCountDate']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'sharedItemsCount': sharedItemsCount,
        'personalItemsCount': personalItemsCount,
        'unacknowledgedItemsCount': unacknowledgedItemsCount,
        'lastCountDate': lastCountDate == null ? null : hrDateToJson(lastCountDate!),
      };
}

/// تأكيد التعميمات في الشهر.
class DepartmentReportCircularSummaryDto {
  const DepartmentReportCircularSummaryDto({
    required this.circularsSentCount,
    required this.recipientsCount,
    required this.acknowledgedCount,
  });

  final int circularsSentCount;
  final int recipientsCount;
  final int acknowledgedCount;

  factory DepartmentReportCircularSummaryDto.fromJson(Map<String, dynamic> json) => DepartmentReportCircularSummaryDto(
        circularsSentCount: ((json['circularsSentCount']) as num).toInt(),
        recipientsCount: ((json['recipientsCount']) as num).toInt(),
        acknowledgedCount: ((json['acknowledgedCount']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'circularsSentCount': circularsSentCount,
        'recipientsCount': recipientsCount,
        'acknowledgedCount': acknowledgedCount,
      };
}

/// لقطة بيانات الشهر. ثابتة بعد التثبيت ولا يمكن تعديلها.
class DepartmentReportSnapshotDto {
  const DepartmentReportSnapshotDto({
    required this.periodFrom,
    required this.periodTo,
    required this.frozenAt,
    required this.employees,
    required this.evaluations,
    required this.custody,
    required this.circulars,
  });

  final DateTime periodFrom;
  final DateTime periodTo;
  final DateTime frozenAt;
  final List<DepartmentReportEmployeeRowDto> employees;
  final DepartmentReportEvaluationSummaryDto evaluations;
  final DepartmentReportCustodySummaryDto custody;
  final DepartmentReportCircularSummaryDto circulars;

  factory DepartmentReportSnapshotDto.fromJson(Map<String, dynamic> json) => DepartmentReportSnapshotDto(
        periodFrom: hrDateFromJson((json['periodFrom']) as String),
        periodTo: hrDateFromJson((json['periodTo']) as String),
        frozenAt: DateTime.parse((json['frozenAt']) as String),
        employees: ((json['employees']) as List).map((e) => DepartmentReportEmployeeRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
        evaluations: DepartmentReportEvaluationSummaryDto.fromJson((json['evaluations']) as Map<String, dynamic>),
        custody: DepartmentReportCustodySummaryDto.fromJson((json['custody']) as Map<String, dynamic>),
        circulars: DepartmentReportCircularSummaryDto.fromJson((json['circulars']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'periodFrom': hrDateToJson(periodFrom),
        'periodTo': hrDateToJson(periodTo),
        'frozenAt': hrDateTimeToJson(frozenAt),
        'employees': employees.map((e) => e.toJson()).toList(),
        'evaluations': evaluations.toJson(),
        'custody': custody.toJson(),
        'circulars': circulars.toJson(),
      };
}

class DepartmentReportDto {
  const DepartmentReportDto({
    required this.id,
    required this.cycleId,
    required this.year,
    required this.month,
    required this.manager,
    this.department,
    required this.status,
    required this.snapshot,
    this.notes,
    this.submittedAt,
    required this.completion,
    required this.concurrencyStamp,
  });

  final String id;
  final String cycleId;
  final int year;
  final int month;
  final UserRefDto manager;
  final DepartmentRefDto? department;
  final DepartmentReportStatus status;
  final DepartmentReportSnapshotDto snapshot;
  final String? notes;
  final DateTime? submittedAt;
  final DepartmentCompletionDto completion;
  final String concurrencyStamp;

  factory DepartmentReportDto.fromJson(Map<String, dynamic> json) => DepartmentReportDto(
        id: (json['id']) as String,
        cycleId: (json['cycleId']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        manager: UserRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        department: json['department'] == null ? null : DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        status: DepartmentReportStatus.fromJson(json['status']),
        snapshot: DepartmentReportSnapshotDto.fromJson((json['snapshot']) as Map<String, dynamic>),
        notes: json['notes'] == null ? null : (json['notes']) as String,
        submittedAt: json['submittedAt'] == null ? null : DateTime.parse((json['submittedAt']) as String),
        completion: DepartmentCompletionDto.fromJson((json['completion']) as Map<String, dynamic>),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'cycleId': cycleId,
        'year': year,
        'month': month,
        'manager': manager.toJson(),
        'department': department == null ? null : department!.toJson(),
        'status': status.toJson(),
        'snapshot': snapshot.toJson(),
        'notes': notes == null ? null : notes!,
        'submittedAt': submittedAt == null ? null : hrDateTimeToJson(submittedAt!),
        'completion': completion.toJson(),
        'concurrencyStamp': concurrencyStamp,
      };
}

class DepartmentReportSummaryDto {
  const DepartmentReportSummaryDto({
    required this.id,
    required this.cycleId,
    required this.year,
    required this.month,
    required this.manager,
    this.department,
    required this.status,
    this.submittedAt,
    required this.completion,
  });

  final String id;
  final String cycleId;
  final int year;
  final int month;
  final UserRefDto manager;
  final DepartmentRefDto? department;
  final DepartmentReportStatus status;
  final DateTime? submittedAt;
  final DepartmentCompletionDto completion;

  factory DepartmentReportSummaryDto.fromJson(Map<String, dynamic> json) => DepartmentReportSummaryDto(
        id: (json['id']) as String,
        cycleId: (json['cycleId']) as String,
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        manager: UserRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        department: json['department'] == null ? null : DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        status: DepartmentReportStatus.fromJson(json['status']),
        submittedAt: json['submittedAt'] == null ? null : DateTime.parse((json['submittedAt']) as String),
        completion: DepartmentCompletionDto.fromJson((json['completion']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'cycleId': cycleId,
        'year': year,
        'month': month,
        'manager': manager.toJson(),
        'department': department == null ? null : department!.toJson(),
        'status': status.toJson(),
        'submittedAt': submittedAt == null ? null : hrDateTimeToJson(submittedAt!),
        'completion': completion.toJson(),
      };
}

class DepartmentReportSummaryPagedResultDto {
  const DepartmentReportSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<DepartmentReportSummaryDto> items;

  factory DepartmentReportSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => DepartmentReportSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => DepartmentReportSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class DepartmentReportNotesPatchDto {
  const DepartmentReportNotesPatchDto({
    this.notes,
    required this.concurrencyStamp,
  });

  final String? notes;
  final String concurrencyStamp;

  factory DepartmentReportNotesPatchDto.fromJson(Map<String, dynamic> json) => DepartmentReportNotesPatchDto(
        notes: json['notes'] == null ? null : (json['notes']) as String,
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'notes': notes == null ? null : notes!,
        'concurrencyStamp': concurrencyStamp,
      };
}

class DepartmentReportSubmitDto {
  const DepartmentReportSubmitDto({
    required this.concurrencyStamp,
  });

  final String concurrencyStamp;

  factory DepartmentReportSubmitDto.fromJson(Map<String, dynamic> json) => DepartmentReportSubmitDto(
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'concurrencyStamp': concurrencyStamp,
      };
}

/// فترة تقرير المراجع.
enum ReviewReportPeriodType {
  daily('daily'),
  weekly('weekly'),
  monthly('monthly'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ReviewReportPeriodType(this.value);
  final String value;

  static ReviewReportPeriodType fromJson(Object? json) {
    for (final v in ReviewReportPeriodType.values) {
      if (v.value == json) return v;
    }
    return ReviewReportPeriodType.$unknown;
  }

  String toJson() => value;
}

/// حالة تقرير المراجع.
enum ReviewReportStatus {
  draft('draft'),
  submitted('submitted'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ReviewReportStatus(this.value);
  final String value;

  static ReviewReportStatus fromJson(Object? json) {
    for (final v in ReviewReportStatus.values) {
      if (v.value == json) return v;
    }
    return ReviewReportStatus.$unknown;
  }

  String toJson() => value;
}

/// فريق واحد في تقرير المراجع: مدير القسم ومن يرتبط به. الصف لقطة من وقت إنشاء التقرير، ولا يحتوي أي بيانات مالية أو بيانات شكاوى.
class ReviewReportTeamDto {
  const ReviewReportTeamDto({
    required this.manager,
    this.department,
    required this.membersCount,
    this.completionPercent,
    this.notes,
    this.projectNotes,
  });

  final EmployeeRefDto manager;
  final DepartmentRefDto? department;
  /// من يرتبط بالمدير وقت إنشاء التقرير، دون المنتهية خدمتهم.
  final int membersCount;
  /// نسبة الإنجاز التي يقدّرها المراجع. null ما دام لم يكتبها.
  final int? completionPercent;
  /// ملاحظات المراجع على أداء المدير والفريق.
  final String? notes;
  /// ملاحظات تخص مشاريع الفريق.
  final String? projectNotes;

  factory ReviewReportTeamDto.fromJson(Map<String, dynamic> json) => ReviewReportTeamDto(
        manager: EmployeeRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        department: json['department'] == null ? null : DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        membersCount: ((json['membersCount']) as num).toInt(),
        completionPercent: json['completionPercent'] == null ? null : ((json['completionPercent']) as num).toInt(),
        notes: json['notes'] == null ? null : (json['notes']) as String,
        projectNotes: json['projectNotes'] == null ? null : (json['projectNotes']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'manager': manager.toJson(),
        'department': department == null ? null : department!.toJson(),
        'membersCount': membersCount,
        'completionPercent': completionPercent == null ? null : completionPercent!,
        'notes': notes == null ? null : notes!,
        'projectNotes': projectNotes == null ? null : projectNotes!,
      };
}

class ReviewReportDto {
  const ReviewReportDto({
    required this.id,
    required this.reviewer,
    required this.periodType,
    required this.periodFrom,
    required this.periodTo,
    required this.status,
    this.summary,
    this.averageCompletion,
    required this.teams,
    this.submittedAt,
    this.viewedAt,
    required this.concurrencyStamp,
  });

  final String id;
  final UserRefDto reviewer;
  final ReviewReportPeriodType periodType;
  final DateTime periodFrom;
  final DateTime periodTo;
  final ReviewReportStatus status;
  /// الملخص العام للتقرير.
  final String? summary;
  /// متوسط completionPercent للفرق التي تحمل قيمة، مقربًا لأقرب عدد صحيح. null إذا لم تحمل أي فريق قيمة.
  final int? averageCompletion;
  final List<ReviewReportTeamDto> teams;
  final DateTime? submittedAt;
  /// أول اطلاع للمدير التنفيذي على التقرير المرفوع.
  final DateTime? viewedAt;
  final String concurrencyStamp;

  factory ReviewReportDto.fromJson(Map<String, dynamic> json) => ReviewReportDto(
        id: (json['id']) as String,
        reviewer: UserRefDto.fromJson((json['reviewer']) as Map<String, dynamic>),
        periodType: ReviewReportPeriodType.fromJson(json['periodType']),
        periodFrom: hrDateFromJson((json['periodFrom']) as String),
        periodTo: hrDateFromJson((json['periodTo']) as String),
        status: ReviewReportStatus.fromJson(json['status']),
        summary: json['summary'] == null ? null : (json['summary']) as String,
        averageCompletion: json['averageCompletion'] == null ? null : ((json['averageCompletion']) as num).toInt(),
        teams: ((json['teams']) as List).map((e) => ReviewReportTeamDto.fromJson((e) as Map<String, dynamic>)).toList(),
        submittedAt: json['submittedAt'] == null ? null : DateTime.parse((json['submittedAt']) as String),
        viewedAt: json['viewedAt'] == null ? null : DateTime.parse((json['viewedAt']) as String),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'reviewer': reviewer.toJson(),
        'periodType': periodType.toJson(),
        'periodFrom': hrDateToJson(periodFrom),
        'periodTo': hrDateToJson(periodTo),
        'status': status.toJson(),
        'summary': summary == null ? null : summary!,
        'averageCompletion': averageCompletion == null ? null : averageCompletion!,
        'teams': teams.map((e) => e.toJson()).toList(),
        'submittedAt': submittedAt == null ? null : hrDateTimeToJson(submittedAt!),
        'viewedAt': viewedAt == null ? null : hrDateTimeToJson(viewedAt!),
        'concurrencyStamp': concurrencyStamp,
      };
}

class ReviewReportSummaryDto {
  const ReviewReportSummaryDto({
    required this.id,
    required this.reviewer,
    required this.periodType,
    required this.periodFrom,
    required this.periodTo,
    required this.status,
    required this.teamsCount,
    this.averageCompletion,
    this.submittedAt,
    this.viewedAt,
  });

  final String id;
  final UserRefDto reviewer;
  final ReviewReportPeriodType periodType;
  final DateTime periodFrom;
  final DateTime periodTo;
  final ReviewReportStatus status;
  final int teamsCount;
  final int? averageCompletion;
  final DateTime? submittedAt;
  final DateTime? viewedAt;

  factory ReviewReportSummaryDto.fromJson(Map<String, dynamic> json) => ReviewReportSummaryDto(
        id: (json['id']) as String,
        reviewer: UserRefDto.fromJson((json['reviewer']) as Map<String, dynamic>),
        periodType: ReviewReportPeriodType.fromJson(json['periodType']),
        periodFrom: hrDateFromJson((json['periodFrom']) as String),
        periodTo: hrDateFromJson((json['periodTo']) as String),
        status: ReviewReportStatus.fromJson(json['status']),
        teamsCount: ((json['teamsCount']) as num).toInt(),
        averageCompletion: json['averageCompletion'] == null ? null : ((json['averageCompletion']) as num).toInt(),
        submittedAt: json['submittedAt'] == null ? null : DateTime.parse((json['submittedAt']) as String),
        viewedAt: json['viewedAt'] == null ? null : DateTime.parse((json['viewedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'reviewer': reviewer.toJson(),
        'periodType': periodType.toJson(),
        'periodFrom': hrDateToJson(periodFrom),
        'periodTo': hrDateToJson(periodTo),
        'status': status.toJson(),
        'teamsCount': teamsCount,
        'averageCompletion': averageCompletion == null ? null : averageCompletion!,
        'submittedAt': submittedAt == null ? null : hrDateTimeToJson(submittedAt!),
        'viewedAt': viewedAt == null ? null : hrDateTimeToJson(viewedAt!),
      };
}

class ReviewReportSummaryPagedResultDto {
  const ReviewReportSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ReviewReportSummaryDto> items;

  factory ReviewReportSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => ReviewReportSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ReviewReportSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class ReviewReportCreateDto {
  const ReviewReportCreateDto({
    required this.periodType,
    required this.periodFrom,
    required this.periodTo,
    this.summary,
  });

  final ReviewReportPeriodType periodType;
  final DateTime periodFrom;
  final DateTime periodTo;
  final String? summary;

  factory ReviewReportCreateDto.fromJson(Map<String, dynamic> json) => ReviewReportCreateDto(
        periodType: ReviewReportPeriodType.fromJson(json['periodType']),
        periodFrom: hrDateFromJson((json['periodFrom']) as String),
        periodTo: hrDateFromJson((json['periodTo']) as String),
        summary: json['summary'] == null ? null : (json['summary']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'periodType': periodType.toJson(),
        'periodFrom': hrDateToJson(periodFrom),
        'periodTo': hrDateToJson(periodTo),
        if (summary != null) 'summary': summary!,
      };
}

class ReviewReportTeamInputDto {
  const ReviewReportTeamInputDto({
    required this.managerEmployeeId,
    this.completionPercent,
    this.notes,
    this.projectNotes,
  });

  final String managerEmployeeId;
  final int? completionPercent;
  final String? notes;
  final String? projectNotes;

  factory ReviewReportTeamInputDto.fromJson(Map<String, dynamic> json) => ReviewReportTeamInputDto(
        managerEmployeeId: (json['managerEmployeeId']) as String,
        completionPercent: json['completionPercent'] == null ? null : ((json['completionPercent']) as num).toInt(),
        notes: json['notes'] == null ? null : (json['notes']) as String,
        projectNotes: json['projectNotes'] == null ? null : (json['projectNotes']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'managerEmployeeId': managerEmployeeId,
        'completionPercent': completionPercent == null ? null : completionPercent!,
        'notes': notes == null ? null : notes!,
        'projectNotes': projectNotes == null ? null : projectNotes!,
      };
}

class ReviewReportUpdateDto {
  const ReviewReportUpdateDto({
    required this.periodType,
    required this.periodFrom,
    required this.periodTo,
    this.summary,
    required this.teams,
    required this.concurrencyStamp,
  });

  final ReviewReportPeriodType periodType;
  final DateTime periodFrom;
  final DateTime periodTo;
  final String? summary;
  final List<ReviewReportTeamInputDto> teams;
  final String concurrencyStamp;

  factory ReviewReportUpdateDto.fromJson(Map<String, dynamic> json) => ReviewReportUpdateDto(
        periodType: ReviewReportPeriodType.fromJson(json['periodType']),
        periodFrom: hrDateFromJson((json['periodFrom']) as String),
        periodTo: hrDateFromJson((json['periodTo']) as String),
        summary: json['summary'] == null ? null : (json['summary']) as String,
        teams: ((json['teams']) as List).map((e) => ReviewReportTeamInputDto.fromJson((e) as Map<String, dynamic>)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'periodType': periodType.toJson(),
        'periodFrom': hrDateToJson(periodFrom),
        'periodTo': hrDateToJson(periodTo),
        'summary': summary == null ? null : summary!,
        'teams': teams.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
      };
}

class ReviewReportSubmitDto {
  const ReviewReportSubmitDto({
    required this.concurrencyStamp,
  });

  final String concurrencyStamp;

  factory ReviewReportSubmitDto.fromJson(Map<String, dynamic> json) => ReviewReportSubmitDto(
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'concurrencyStamp': concurrencyStamp,
      };
}

/// رموز التقارير القابلة للتصدير. التقارير المالية للمحاسب في وحدة أخرى وليست هنا.
enum ReportCode {
  teamAttendance('teamAttendance'),
  teamLatenessAbsence('teamLatenessAbsence'),
  teamLeaves('teamLeaves'),
  teamPending('teamPending'),
  teamCustody('teamCustody'),
  teamEvaluations('teamEvaluations'),
  reviewDashboard('reviewDashboard'),
  evaluationConsistency('evaluationConsistency'),
  hrAttendance('hrAttendance'),
  hrLeaveBalances('hrLeaveBalances'),
  hrRequests('hrRequests'),
  hrContracts('hrContracts'),
  hrWarnings('hrWarnings'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ReportCode(this.value);
  final String value;

  static ReportCode fromJson(Object? json) {
    for (final v in ReportCode.values) {
      if (v.value == json) return v;
    }
    return ReportCode.$unknown;
  }

  String toJson() => value;
}

/// صيغة التصدير.
enum ReportExportFormat {
  pdf('pdf'),
  excel('excel'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ReportExportFormat(this.value);
  final String value;

  static ReportExportFormat fromJson(Object? json) {
    for (final v in ReportExportFormat.values) {
      if (v.value == json) return v;
    }
    return ReportExportFormat.$unknown;
  }

  String toJson() => value;
}

/// عهدة شخصية باسم الموظف، أو مشتركة باسم القسم.
enum ReportCustodyKind {
  personal('personal'),
  shared('shared'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ReportCustodyKind(this.value);
  final String value;

  static ReportCustodyKind fromJson(Object? json) {
    for (final v in ReportCustodyKind.values) {
      if (v.value == json) return v;
    }
    return ReportCustodyKind.$unknown;
  }

  String toJson() => value;
}

/// يوم حضور لموظف في الفريق. لا يحتوي بيانات مالية.
class TeamAttendanceRowDto {
  const TeamAttendanceRowDto({
    required this.employee,
    required this.date,
    this.checkIn,
    this.checkOut,
    required this.lateMinutes,
    required this.statusName,
  });

  final EmployeeRefDto employee;
  final DateTime date;
  final DateTime? checkIn;
  final DateTime? checkOut;
  final int lateMinutes;
  /// اسم الحالة مترجم، للعرض فقط.
  final String statusName;

  factory TeamAttendanceRowDto.fromJson(Map<String, dynamic> json) => TeamAttendanceRowDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        date: hrDateFromJson((json['date']) as String),
        checkIn: json['checkIn'] == null ? null : DateTime.parse((json['checkIn']) as String),
        checkOut: json['checkOut'] == null ? null : DateTime.parse((json['checkOut']) as String),
        lateMinutes: ((json['lateMinutes']) as num).toInt(),
        statusName: (json['statusName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'date': hrDateToJson(date),
        'checkIn': checkIn == null ? null : hrDateTimeToJson(checkIn!),
        'checkOut': checkOut == null ? null : hrDateTimeToJson(checkOut!),
        'lateMinutes': lateMinutes,
        'statusName': statusName,
      };
}

class TeamAttendanceRowPagedResultDto {
  const TeamAttendanceRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<TeamAttendanceRowDto> items;

  factory TeamAttendanceRowPagedResultDto.fromJson(Map<String, dynamic> json) => TeamAttendanceRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => TeamAttendanceRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// مجموع التأخر والغياب لموظف في الفترة.
class TeamLatenessAbsenceRowDto {
  const TeamLatenessAbsenceRowDto({
    required this.employee,
    required this.lateCount,
    required this.lateMinutes,
    required this.absentDays,
  });

  final EmployeeRefDto employee;
  final int lateCount;
  final int lateMinutes;
  final int absentDays;

  factory TeamLatenessAbsenceRowDto.fromJson(Map<String, dynamic> json) => TeamLatenessAbsenceRowDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        lateCount: ((json['lateCount']) as num).toInt(),
        lateMinutes: ((json['lateMinutes']) as num).toInt(),
        absentDays: ((json['absentDays']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'lateCount': lateCount,
        'lateMinutes': lateMinutes,
        'absentDays': absentDays,
      };
}

class TeamLatenessAbsenceRowPagedResultDto {
  const TeamLatenessAbsenceRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<TeamLatenessAbsenceRowDto> items;

  factory TeamLatenessAbsenceRowPagedResultDto.fromJson(Map<String, dynamic> json) => TeamLatenessAbsenceRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => TeamLatenessAbsenceRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// إجازة لموظف في الفريق.
class TeamLeaveRowDto {
  const TeamLeaveRowDto({
    required this.employee,
    required this.leaveTypeName,
    required this.from,
    required this.to,
    this.days,
    this.hours,
    required this.statusName,
  });

  final EmployeeRefDto employee;
  final String leaveTypeName;
  final DateTime from;
  final DateTime to;
  final double? days;
  final double? hours;
  final String statusName;

  factory TeamLeaveRowDto.fromJson(Map<String, dynamic> json) => TeamLeaveRowDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        leaveTypeName: (json['leaveTypeName']) as String,
        from: DateTime.parse((json['from']) as String),
        to: DateTime.parse((json['to']) as String),
        days: json['days'] == null ? null : ((json['days']) as num).toDouble(),
        hours: json['hours'] == null ? null : ((json['hours']) as num).toDouble(),
        statusName: (json['statusName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'leaveTypeName': leaveTypeName,
        'from': hrDateTimeToJson(from),
        'to': hrDateTimeToJson(to),
        'days': days == null ? null : days!,
        'hours': hours == null ? null : hours!,
        'statusName': statusName,
      };
}

class TeamLeaveRowPagedResultDto {
  const TeamLeaveRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<TeamLeaveRowDto> items;

  factory TeamLeaveRowPagedResultDto.fromJson(Map<String, dynamic> json) => TeamLeaveRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => TeamLeaveRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// طلب معلق لموظف في الفريق.
class TeamPendingRequestRowDto {
  const TeamPendingRequestRowDto({
    required this.requestId,
    required this.employee,
    required this.requestTypeName,
    required this.statusName,
    required this.creationTime,
  });

  final String requestId;
  final EmployeeRefDto employee;
  final String requestTypeName;
  final String statusName;
  final DateTime creationTime;

  factory TeamPendingRequestRowDto.fromJson(Map<String, dynamic> json) => TeamPendingRequestRowDto(
        requestId: (json['requestId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        requestTypeName: (json['requestTypeName']) as String,
        statusName: (json['statusName']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'requestId': requestId,
        'employee': employee.toJson(),
        'requestTypeName': requestTypeName,
        'statusName': statusName,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class TeamPendingRequestRowPagedResultDto {
  const TeamPendingRequestRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<TeamPendingRequestRowDto> items;

  factory TeamPendingRequestRowPagedResultDto.fromJson(Map<String, dynamic> json) => TeamPendingRequestRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => TeamPendingRequestRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// عهدة في الفريق أو القسم.
class TeamCustodyRowDto {
  const TeamCustodyRowDto({
    required this.custodyId,
    required this.itemName,
    required this.kind,
    this.employee,
    required this.assignedAt,
    this.acknowledgedAt,
    required this.statusName,
  });

  final String custodyId;
  final String itemName;
  final ReportCustodyKind kind;
  /// null للعهدة المشتركة.
  final EmployeeRefDto? employee;
  final DateTime assignedAt;
  final DateTime? acknowledgedAt;
  final String statusName;

  factory TeamCustodyRowDto.fromJson(Map<String, dynamic> json) => TeamCustodyRowDto(
        custodyId: (json['custodyId']) as String,
        itemName: (json['itemName']) as String,
        kind: ReportCustodyKind.fromJson(json['kind']),
        employee: json['employee'] == null ? null : EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        assignedAt: DateTime.parse((json['assignedAt']) as String),
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
        statusName: (json['statusName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'custodyId': custodyId,
        'itemName': itemName,
        'kind': kind.toJson(),
        'employee': employee == null ? null : employee!.toJson(),
        'assignedAt': hrDateTimeToJson(assignedAt),
        'acknowledgedAt': acknowledgedAt == null ? null : hrDateTimeToJson(acknowledgedAt!),
        'statusName': statusName,
      };
}

class TeamCustodyRowPagedResultDto {
  const TeamCustodyRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<TeamCustodyRowDto> items;

  factory TeamCustodyRowPagedResultDto.fromJson(Map<String, dynamic> json) => TeamCustodyRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => TeamCustodyRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// تقييم مرفوع لموظف في الفريق. لا يحتوي الملاحظة الداخلية ولا نصوص الملاحظات.
class TeamEvaluationRowDto {
  const TeamEvaluationRowDto({
    required this.evaluationId,
    required this.employee,
    required this.year,
    required this.month,
    required this.status,
    required this.objectionStatus,
    required this.scores,
  });

  final String evaluationId;
  final EmployeeRefDto employee;
  final int year;
  final int month;
  final EvaluationStatus status;
  final EvaluationObjectionStatus objectionStatus;
  final List<EvaluationSubmittedScoreDto> scores;

  factory TeamEvaluationRowDto.fromJson(Map<String, dynamic> json) => TeamEvaluationRowDto(
        evaluationId: (json['evaluationId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        status: EvaluationStatus.fromJson(json['status']),
        objectionStatus: EvaluationObjectionStatus.fromJson(json['objectionStatus']),
        scores: ((json['scores']) as List).map((e) => EvaluationSubmittedScoreDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'evaluationId': evaluationId,
        'employee': employee.toJson(),
        'year': year,
        'month': month,
        'status': status.toJson(),
        'objectionStatus': objectionStatus.toJson(),
        'scores': scores.map((e) => e.toJson()).toList(),
      };
}

class TeamEvaluationRowPagedResultDto {
  const TeamEvaluationRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<TeamEvaluationRowDto> items;

  factory TeamEvaluationRowPagedResultDto.fromJson(Map<String, dynamic> json) => TeamEvaluationRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => TeamEvaluationRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// أعداد فقط على مستوى الشركة، دون أسماء أو تفاصيل أو أقسام. تستبعد كل شكوى يكون المراجع طرفًا فيها، دون أي حقل يكشف عدد المستبعد.
class ReviewComplaintCountsDto {
  const ReviewComplaintCountsDto({
    required this.confidentialSubmittedCount,
    required this.highlyConfidentialSubmittedCount,
    required this.lateNormalCount,
    required this.lateConfidentialCount,
    required this.lateHighlyConfidentialCount,
    required this.asOf,
  });

  /// الشكاوى السرية المقدمة في الشهر المطلوب.
  final int confidentialSubmittedCount;
  /// الشكاوى السرية جدًا المقدمة في الشهر المطلوب.
  final int highlyConfidentialSubmittedCount;
  /// الشكاوى العادية المتأخرة حاليًا.
  final int lateNormalCount;
  /// الشكاوى السرية المتأخرة حاليًا.
  final int lateConfidentialCount;
  /// الشكاوى السرية جدًا المتأخرة حاليًا.
  final int lateHighlyConfidentialCount;
  final DateTime asOf;

  factory ReviewComplaintCountsDto.fromJson(Map<String, dynamic> json) => ReviewComplaintCountsDto(
        confidentialSubmittedCount: ((json['confidentialSubmittedCount']) as num).toInt(),
        highlyConfidentialSubmittedCount: ((json['highlyConfidentialSubmittedCount']) as num).toInt(),
        lateNormalCount: ((json['lateNormalCount']) as num).toInt(),
        lateConfidentialCount: ((json['lateConfidentialCount']) as num).toInt(),
        lateHighlyConfidentialCount: ((json['lateHighlyConfidentialCount']) as num).toInt(),
        asOf: DateTime.parse((json['asOf']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'confidentialSubmittedCount': confidentialSubmittedCount,
        'highlyConfidentialSubmittedCount': highlyConfidentialSubmittedCount,
        'lateNormalCount': lateNormalCount,
        'lateConfidentialCount': lateConfidentialCount,
        'lateHighlyConfidentialCount': lateHighlyConfidentialCount,
        'asOf': hrDateTimeToJson(asOf),
      };
}

/// إنجاز مدير قسم في الدورة.
class ReviewDepartmentCompletionDto {
  const ReviewDepartmentCompletionDto({
    required this.manager,
    this.department,
    required this.completion,
  });

  final UserRefDto manager;
  final DepartmentRefDto? department;
  final DepartmentCompletionDto completion;

  factory ReviewDepartmentCompletionDto.fromJson(Map<String, dynamic> json) => ReviewDepartmentCompletionDto(
        manager: UserRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        department: json['department'] == null ? null : DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        completion: DepartmentCompletionDto.fromJson((json['completion']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'manager': manager.toJson(),
        'department': department == null ? null : department!.toJson(),
        'completion': completion.toJson(),
      };
}

/// لوحة المراجع. لا تحتوي أسماء موظفين أو درجات فردية أو أي محتوى شكوى.
class ReviewDashboardDto {
  const ReviewDashboardDto({
    required this.year,
    required this.month,
    required this.complaintCounts,
    this.cycleId,
    required this.completedDepartmentsCount,
    required this.lateDepartmentsCount,
    required this.departments,
  });

  final int year;
  final int month;
  final ReviewComplaintCountsDto complaintCounts;
  final String? cycleId;
  final int completedDepartmentsCount;
  final int lateDepartmentsCount;
  final List<ReviewDepartmentCompletionDto> departments;

  factory ReviewDashboardDto.fromJson(Map<String, dynamic> json) => ReviewDashboardDto(
        year: ((json['year']) as num).toInt(),
        month: ((json['month']) as num).toInt(),
        complaintCounts: ReviewComplaintCountsDto.fromJson((json['complaintCounts']) as Map<String, dynamic>),
        cycleId: json['cycleId'] == null ? null : (json['cycleId']) as String,
        completedDepartmentsCount: ((json['completedDepartmentsCount']) as num).toInt(),
        lateDepartmentsCount: ((json['lateDepartmentsCount']) as num).toInt(),
        departments: ((json['departments']) as List).map((e) => ReviewDepartmentCompletionDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'year': year,
        'month': month,
        'complaintCounts': complaintCounts.toJson(),
        'cycleId': cycleId == null ? null : cycleId!,
        'completedDepartmentsCount': completedDepartmentsCount,
        'lateDepartmentsCount': lateDepartmentsCount,
        'departments': departments.map((e) => e.toJson()).toList(),
      };
}

/// اتساق تقييمات مدير في الدورة. مجمّع، دون أسماء الموظفين ولا درجاتهم الفردية ولا الملاحظات الداخلية.
class EvaluationConsistencyRowDto {
  const EvaluationConsistencyRowDto({
    required this.manager,
    this.department,
    required this.expectedCount,
    required this.submittedCount,
    required this.completionRatio,
    required this.distinctGradesUsed,
    required this.sameGradeForAll,
    required this.gradeCounts,
  });

  final UserRefDto manager;
  final DepartmentRefDto? department;
  final int expectedCount;
  final int submittedCount;
  /// نسبة الإنجاز من 0 إلى 1. تخص إنجاز النماذج فقط، وليست درجة تقييم.
  final double completionRatio;
  final int distinctGradesUsed;
  /// true إذا أعطى المدير الدرجة نفسها لكل موظفيه في كل المعايير، بشرط رفع تقييمين على الأقل.
  final bool sameGradeForAll;
  final List<EvaluationGradeCountDto> gradeCounts;

  factory EvaluationConsistencyRowDto.fromJson(Map<String, dynamic> json) => EvaluationConsistencyRowDto(
        manager: UserRefDto.fromJson((json['manager']) as Map<String, dynamic>),
        department: json['department'] == null ? null : DepartmentRefDto.fromJson((json['department']) as Map<String, dynamic>),
        expectedCount: ((json['expectedCount']) as num).toInt(),
        submittedCount: ((json['submittedCount']) as num).toInt(),
        completionRatio: ((json['completionRatio']) as num).toDouble(),
        distinctGradesUsed: ((json['distinctGradesUsed']) as num).toInt(),
        sameGradeForAll: (json['sameGradeForAll']) as bool,
        gradeCounts: ((json['gradeCounts']) as List).map((e) => EvaluationGradeCountDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'manager': manager.toJson(),
        'department': department == null ? null : department!.toJson(),
        'expectedCount': expectedCount,
        'submittedCount': submittedCount,
        'completionRatio': completionRatio,
        'distinctGradesUsed': distinctGradesUsed,
        'sameGradeForAll': sameGradeForAll,
        'gradeCounts': gradeCounts.map((e) => e.toJson()).toList(),
      };
}

class EvaluationConsistencyRowPagedResultDto {
  const EvaluationConsistencyRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EvaluationConsistencyRowDto> items;

  factory EvaluationConsistencyRowPagedResultDto.fromJson(Map<String, dynamic> json) => EvaluationConsistencyRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EvaluationConsistencyRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// حضور موظف في الفترة. لا يحتوي بيانات مالية.
class HrAttendanceReportRowDto {
  const HrAttendanceReportRowDto({
    required this.employee,
    required this.scheduledWorkingDays,
    required this.presentDays,
    required this.absentDays,
    required this.lateCount,
    required this.lateMinutes,
    required this.leaveDays,
  });

  final EmployeeRefDto employee;
  final int scheduledWorkingDays;
  final int presentDays;
  final int absentDays;
  final int lateCount;
  final int lateMinutes;
  final double leaveDays;

  factory HrAttendanceReportRowDto.fromJson(Map<String, dynamic> json) => HrAttendanceReportRowDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        scheduledWorkingDays: ((json['scheduledWorkingDays']) as num).toInt(),
        presentDays: ((json['presentDays']) as num).toInt(),
        absentDays: ((json['absentDays']) as num).toInt(),
        lateCount: ((json['lateCount']) as num).toInt(),
        lateMinutes: ((json['lateMinutes']) as num).toInt(),
        leaveDays: ((json['leaveDays']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'scheduledWorkingDays': scheduledWorkingDays,
        'presentDays': presentDays,
        'absentDays': absentDays,
        'lateCount': lateCount,
        'lateMinutes': lateMinutes,
        'leaveDays': leaveDays,
      };
}

class HrAttendanceReportRowPagedResultDto {
  const HrAttendanceReportRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<HrAttendanceReportRowDto> items;

  factory HrAttendanceReportRowPagedResultDto.fromJson(Map<String, dynamic> json) => HrAttendanceReportRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => HrAttendanceReportRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// رصيد موظف.
class HrLeaveBalanceReportRowDto {
  const HrLeaveBalanceReportRowDto({
    required this.employee,
    required this.leaveYearStart,
    required this.entitlementDays,
    required this.carriedInDays,
    required this.usedDays,
    required this.heldDays,
    required this.remainingDays,
    required this.usedHours,
  });

  final EmployeeRefDto employee;
  final DateTime leaveYearStart;
  final double entitlementDays;
  final double carriedInDays;
  final double usedDays;
  final double heldDays;
  final double remainingDays;
  final double usedHours;

  factory HrLeaveBalanceReportRowDto.fromJson(Map<String, dynamic> json) => HrLeaveBalanceReportRowDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        leaveYearStart: hrDateFromJson((json['leaveYearStart']) as String),
        entitlementDays: ((json['entitlementDays']) as num).toDouble(),
        carriedInDays: ((json['carriedInDays']) as num).toDouble(),
        usedDays: ((json['usedDays']) as num).toDouble(),
        heldDays: ((json['heldDays']) as num).toDouble(),
        remainingDays: ((json['remainingDays']) as num).toDouble(),
        usedHours: ((json['usedHours']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'leaveYearStart': hrDateToJson(leaveYearStart),
        'entitlementDays': entitlementDays,
        'carriedInDays': carriedInDays,
        'usedDays': usedDays,
        'heldDays': heldDays,
        'remainingDays': remainingDays,
        'usedHours': usedHours,
      };
}

class HrLeaveBalanceReportRowPagedResultDto {
  const HrLeaveBalanceReportRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<HrLeaveBalanceReportRowDto> items;

  factory HrLeaveBalanceReportRowPagedResultDto.fromJson(Map<String, dynamic> json) => HrLeaveBalanceReportRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => HrLeaveBalanceReportRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// طلب في تقرير الموارد البشرية.
class HrRequestReportRowDto {
  const HrRequestReportRowDto({
    required this.requestId,
    required this.employee,
    required this.requestTypeName,
    required this.statusName,
    required this.creationTime,
    this.closedAt,
  });

  final String requestId;
  final EmployeeRefDto employee;
  final String requestTypeName;
  final String statusName;
  final DateTime creationTime;
  final DateTime? closedAt;

  factory HrRequestReportRowDto.fromJson(Map<String, dynamic> json) => HrRequestReportRowDto(
        requestId: (json['requestId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        requestTypeName: (json['requestTypeName']) as String,
        statusName: (json['statusName']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
        closedAt: json['closedAt'] == null ? null : DateTime.parse((json['closedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'requestId': requestId,
        'employee': employee.toJson(),
        'requestTypeName': requestTypeName,
        'statusName': statusName,
        'creationTime': hrDateTimeToJson(creationTime),
        'closedAt': closedAt == null ? null : hrDateTimeToJson(closedAt!),
      };
}

class HrRequestReportRowPagedResultDto {
  const HrRequestReportRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<HrRequestReportRowDto> items;

  factory HrRequestReportRowPagedResultDto.fromJson(Map<String, dynamic> json) => HrRequestReportRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => HrRequestReportRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// عقد في تقرير الموارد البشرية.
class HrContractReportRowDto {
  const HrContractReportRowDto({
    required this.contractId,
    required this.employee,
    required this.typeName,
    required this.startDate,
    required this.endDate,
    required this.statusName,
  });

  final String contractId;
  final EmployeeRefDto employee;
  final String typeName;
  final DateTime startDate;
  final DateTime endDate;
  final String statusName;

  factory HrContractReportRowDto.fromJson(Map<String, dynamic> json) => HrContractReportRowDto(
        contractId: (json['contractId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        typeName: (json['typeName']) as String,
        startDate: hrDateFromJson((json['startDate']) as String),
        endDate: hrDateFromJson((json['endDate']) as String),
        statusName: (json['statusName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'contractId': contractId,
        'employee': employee.toJson(),
        'typeName': typeName,
        'startDate': hrDateToJson(startDate),
        'endDate': hrDateToJson(endDate),
        'statusName': statusName,
      };
}

class HrContractReportRowPagedResultDto {
  const HrContractReportRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<HrContractReportRowDto> items;

  factory HrContractReportRowPagedResultDto.fromJson(Map<String, dynamic> json) => HrContractReportRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => HrContractReportRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// إنذار في تقرير الموارد البشرية، دون وصف الواقعة.
class HrWarningReportRowDto {
  const HrWarningReportRowDto({
    required this.warningId,
    required this.employee,
    required this.levelName,
    required this.incidentDate,
    required this.statusName,
  });

  final String warningId;
  final EmployeeRefDto employee;
  final String levelName;
  final DateTime incidentDate;
  final String statusName;

  factory HrWarningReportRowDto.fromJson(Map<String, dynamic> json) => HrWarningReportRowDto(
        warningId: (json['warningId']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        levelName: (json['levelName']) as String,
        incidentDate: hrDateFromJson((json['incidentDate']) as String),
        statusName: (json['statusName']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'warningId': warningId,
        'employee': employee.toJson(),
        'levelName': levelName,
        'incidentDate': hrDateToJson(incidentDate),
        'statusName': statusName,
      };
}

class HrWarningReportRowPagedResultDto {
  const HrWarningReportRowPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<HrWarningReportRowDto> items;

  factory HrWarningReportRowPagedResultDto.fromJson(Map<String, dynamic> json) => HrWarningReportRowPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => HrWarningReportRowDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// team: موظفو المدير المرسل. departmentManagers: مديرو الأقسام. company: كل الشركة.
enum CircularScope {
  team('team'),
  departmentManagers('departmentManagers'),
  company('company'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const CircularScope(this.value);
  final String value;

  static CircularScope fromJson(Object? json) {
    for (final v in CircularScope.values) {
      if (v.value == json) return v;
    }
    return CircularScope.$unknown;
  }

  String toJson() => value;
}

/// لا يحدد المرسل المستلمين يدويًا. يحسبهم الخادم من scope.
class CircularCreateDto {
  const CircularCreateDto({
    required this.scope,
    required this.title,
    required this.content,
  });

  final CircularScope scope;
  final String title;
  final String content;

  factory CircularCreateDto.fromJson(Map<String, dynamic> json) => CircularCreateDto(
        scope: CircularScope.fromJson(json['scope']),
        title: (json['title']) as String,
        content: (json['content']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'scope': scope.toJson(),
        'title': title,
        'content': content,
      };
}

/// تعميم وصل إلى المستخدم.
class CircularReceivedDto {
  const CircularReceivedDto({
    required this.id,
    required this.scope,
    required this.title,
    required this.content,
    required this.sender,
    required this.sentAt,
    required this.acknowledgementRequired,
    this.acknowledgedAt,
  });

  final String id;
  final CircularScope scope;
  final String title;
  final String content;
  final UserRefDto sender;
  final DateTime sentAt;
  /// Always true: acknowledgement is required on every circular.
  final bool acknowledgementRequired;
  final DateTime? acknowledgedAt;

  factory CircularReceivedDto.fromJson(Map<String, dynamic> json) => CircularReceivedDto(
        id: (json['id']) as String,
        scope: CircularScope.fromJson(json['scope']),
        title: (json['title']) as String,
        content: (json['content']) as String,
        sender: UserRefDto.fromJson((json['sender']) as Map<String, dynamic>),
        sentAt: DateTime.parse((json['sentAt']) as String),
        acknowledgementRequired: (json['acknowledgementRequired']) as bool,
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'scope': scope.toJson(),
        'title': title,
        'content': content,
        'sender': sender.toJson(),
        'sentAt': hrDateTimeToJson(sentAt),
        'acknowledgementRequired': acknowledgementRequired,
        'acknowledgedAt': acknowledgedAt == null ? null : hrDateTimeToJson(acknowledgedAt!),
      };
}

class CircularReceivedPagedResultDto {
  const CircularReceivedPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<CircularReceivedDto> items;

  factory CircularReceivedPagedResultDto.fromJson(Map<String, dynamic> json) => CircularReceivedPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => CircularReceivedDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// تعميم أرسله المستخدم.
class CircularSentDto {
  const CircularSentDto({
    required this.id,
    required this.scope,
    required this.title,
    required this.content,
    required this.sentAt,
    required this.recipientsCount,
    required this.acknowledgedCount,
  });

  final String id;
  final CircularScope scope;
  final String title;
  final String content;
  final DateTime sentAt;
  final int recipientsCount;
  final int acknowledgedCount;

  factory CircularSentDto.fromJson(Map<String, dynamic> json) => CircularSentDto(
        id: (json['id']) as String,
        scope: CircularScope.fromJson(json['scope']),
        title: (json['title']) as String,
        content: (json['content']) as String,
        sentAt: DateTime.parse((json['sentAt']) as String),
        recipientsCount: ((json['recipientsCount']) as num).toInt(),
        acknowledgedCount: ((json['acknowledgedCount']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'scope': scope.toJson(),
        'title': title,
        'content': content,
        'sentAt': hrDateTimeToJson(sentAt),
        'recipientsCount': recipientsCount,
        'acknowledgedCount': acknowledgedCount,
      };
}

class CircularSentPagedResultDto {
  const CircularSentPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<CircularSentDto> items;

  factory CircularSentPagedResultDto.fromJson(Map<String, dynamic> json) => CircularSentPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => CircularSentDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// حالة استلام مستلم واحد.
class CircularReceiptDto {
  const CircularReceiptDto({
    required this.recipient,
    this.acknowledgedAt,
  });

  final EmployeeRefDto recipient;
  final DateTime? acknowledgedAt;

  factory CircularReceiptDto.fromJson(Map<String, dynamic> json) => CircularReceiptDto(
        recipient: EmployeeRefDto.fromJson((json['recipient']) as Map<String, dynamic>),
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'recipient': recipient.toJson(),
        'acknowledgedAt': acknowledgedAt == null ? null : hrDateTimeToJson(acknowledgedAt!),
      };
}

class CircularReceiptPagedResultDto {
  const CircularReceiptPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<CircularReceiptDto> items;

  factory CircularReceiptPagedResultDto.fromJson(Map<String, dynamic> json) => CircularReceiptPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => CircularReceiptDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// active: ساري وخارج فترة التنبيه. pendingRecommendation: داخل فترة التنبيه وبانتظار توصية الموارد البشرية. pendingDecision: رُفعت التوصية وبانتظار القرار. renewed وnotRenewed: حالتان نهائيتان بعد القرار.
enum ContractStatus {
  active('active'),
  pendingRecommendation('pendingRecommendation'),
  pendingDecision('pendingDecision'),
  renewed('renewed'),
  notRenewed('notRenewed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ContractStatus(this.value);
  final String value;

  static ContractStatus fromJson(Object? json) {
    for (final v in ContractStatus.values) {
      if (v.value == json) return v;
    }
    return ContractStatus.$unknown;
  }

  String toJson() => value;
}

/// الحالة التي يراها الموظف. مراحل التوصية الداخلية تظهر له active.
enum ContractEmployeeStatus {
  active('active'),
  renewed('renewed'),
  notRenewed('notRenewed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ContractEmployeeStatus(this.value);
  final String value;

  static ContractEmployeeStatus fromJson(Object? json) {
    for (final v in ContractEmployeeStatus.values) {
      if (v.value == json) return v;
    }
    return ContractEmployeeStatus.$unknown;
  }

  String toJson() => value;
}

enum ContractRenewalOption {
  renew('renew'),
  doNotRenew('doNotRenew'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ContractRenewalOption(this.value);
  final String value;

  static ContractRenewalOption fromJson(Object? json) {
    for (final v in ContractRenewalOption.values) {
      if (v.value == json) return v;
    }
    return ContractRenewalOption.$unknown;
  }

  String toJson() => value;
}

class ContractCreateInputDto {
  const ContractCreateInputDto({
    required this.employeeId,
    required this.contractType,
    required this.startDate,
    required this.endDate,
    this.previousContractId,
  });

  final String employeeId;
  /// نوع العقد كما تدخله الموارد البشرية.
  final String contractType;
  final DateTime startDate;
  /// يجب أن يكون بعد startDate.
  final DateTime endDate;
  /// العقد السابق عند إنشاء عقد التجديد. يجب أن تكون حالته renewed ولنفس الموظف.
  final String? previousContractId;

  factory ContractCreateInputDto.fromJson(Map<String, dynamic> json) => ContractCreateInputDto(
        employeeId: (json['employeeId']) as String,
        contractType: (json['contractType']) as String,
        startDate: hrDateFromJson((json['startDate']) as String),
        endDate: hrDateFromJson((json['endDate']) as String),
        previousContractId: json['previousContractId'] == null ? null : (json['previousContractId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employeeId': employeeId,
        'contractType': contractType,
        'startDate': hrDateToJson(startDate),
        'endDate': hrDateToJson(endDate),
        if (previousContractId != null) 'previousContractId': previousContractId!,
      };
}

class ContractRecommendationDto {
  const ContractRecommendationDto({
    required this.employeePosition,
    required this.recommendation,
    this.note,
    required this.recommendedBy,
    required this.recommendedAt,
  });

  /// موقف الموظف الذي ترفعه الموارد البشرية.
  final String employeePosition;
  final ContractRenewalOption recommendation;
  final String? note;
  final UserRefDto recommendedBy;
  final DateTime recommendedAt;

  factory ContractRecommendationDto.fromJson(Map<String, dynamic> json) => ContractRecommendationDto(
        employeePosition: (json['employeePosition']) as String,
        recommendation: ContractRenewalOption.fromJson(json['recommendation']),
        note: json['note'] == null ? null : (json['note']) as String,
        recommendedBy: UserRefDto.fromJson((json['recommendedBy']) as Map<String, dynamic>),
        recommendedAt: DateTime.parse((json['recommendedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employeePosition': employeePosition,
        'recommendation': recommendation.toJson(),
        'note': note == null ? null : note!,
        'recommendedBy': recommendedBy.toJson(),
        'recommendedAt': hrDateTimeToJson(recommendedAt),
      };
}

class ContractDecisionDto {
  const ContractDecisionDto({
    required this.decision,
    this.note,
    this.employeeMessage,
    required this.decidedBy,
    required this.decidedAt,
  });

  final ContractRenewalOption decision;
  /// ملاحظة داخلية لا تصل إلى الموظف.
  final String? note;
  /// النص الذي يراه الموظف.
  final String? employeeMessage;
  final UserRefDto decidedBy;
  final DateTime decidedAt;

  factory ContractDecisionDto.fromJson(Map<String, dynamic> json) => ContractDecisionDto(
        decision: ContractRenewalOption.fromJson(json['decision']),
        note: json['note'] == null ? null : (json['note']) as String,
        employeeMessage: json['employeeMessage'] == null ? null : (json['employeeMessage']) as String,
        decidedBy: UserRefDto.fromJson((json['decidedBy']) as Map<String, dynamic>),
        decidedAt: DateTime.parse((json['decidedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'decision': decision.toJson(),
        'note': note == null ? null : note!,
        'employeeMessage': employeeMessage == null ? null : employeeMessage!,
        'decidedBy': decidedBy.toJson(),
        'decidedAt': hrDateTimeToJson(decidedAt),
      };
}

/// عرض الموارد البشرية للعقد. لا يتضمن أي بيانات مالية.
class ContractDto {
  const ContractDto({
    required this.id,
    required this.employee,
    required this.contractType,
    required this.startDate,
    required this.endDate,
    this.previousContractId,
    required this.status,
    required this.daysRemaining,
    required this.isExpired,
    this.recommendation,
    this.decision,
    this.decisionDueDate,
    required this.isDecisionOverdue,
    this.employeeNotifiedAt,
    required this.creationTime,
    this.lastModificationTime,
  });

  final String id;
  final EmployeeRefDto employee;
  final String contractType;
  final DateTime startDate;
  final DateTime endDate;
  final String? previousContractId;
  final ContractStatus status;
  /// الأيام التقويمية حتى endDate. تكون سالبة بعد انتهاء العقد.
  final int daysRemaining;
  /// true إذا مضى endDate.
  final bool isExpired;
  final ContractRecommendationDto? recommendation;
  final ContractDecisionDto? decision;
  /// موعد القرار: 5 أيام دوام من رفع التوصية.
  final DateTime? decisionDueDate;
  /// true إذا مضى decisionDueDate دون قرار. لا يترتب عليه إجراء تلقائي.
  final bool isDecisionOverdue;
  /// وقت إبلاغ الموظف بالقرار.
  final DateTime? employeeNotifiedAt;
  final DateTime creationTime;
  final DateTime? lastModificationTime;

  factory ContractDto.fromJson(Map<String, dynamic> json) => ContractDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        contractType: (json['contractType']) as String,
        startDate: hrDateFromJson((json['startDate']) as String),
        endDate: hrDateFromJson((json['endDate']) as String),
        previousContractId: json['previousContractId'] == null ? null : (json['previousContractId']) as String,
        status: ContractStatus.fromJson(json['status']),
        daysRemaining: ((json['daysRemaining']) as num).toInt(),
        isExpired: (json['isExpired']) as bool,
        recommendation: json['recommendation'] == null ? null : ContractRecommendationDto.fromJson((json['recommendation']) as Map<String, dynamic>),
        decision: json['decision'] == null ? null : ContractDecisionDto.fromJson((json['decision']) as Map<String, dynamic>),
        decisionDueDate: json['decisionDueDate'] == null ? null : hrDateFromJson((json['decisionDueDate']) as String),
        isDecisionOverdue: (json['isDecisionOverdue']) as bool,
        employeeNotifiedAt: json['employeeNotifiedAt'] == null ? null : DateTime.parse((json['employeeNotifiedAt']) as String),
        creationTime: DateTime.parse((json['creationTime']) as String),
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'contractType': contractType,
        'startDate': hrDateToJson(startDate),
        'endDate': hrDateToJson(endDate),
        'previousContractId': previousContractId == null ? null : previousContractId!,
        'status': status.toJson(),
        'daysRemaining': daysRemaining,
        'isExpired': isExpired,
        'recommendation': recommendation == null ? null : recommendation!.toJson(),
        'decision': decision == null ? null : decision!.toJson(),
        'decisionDueDate': decisionDueDate == null ? null : hrDateToJson(decisionDueDate!),
        'isDecisionOverdue': isDecisionOverdue,
        'employeeNotifiedAt': employeeNotifiedAt == null ? null : hrDateTimeToJson(employeeNotifiedAt!),
        'creationTime': hrDateTimeToJson(creationTime),
        'lastModificationTime': lastModificationTime == null ? null : hrDateTimeToJson(lastModificationTime!),
      };
}

class ContractPagedResultDto {
  const ContractPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ContractDto> items;

  factory ContractPagedResultDto.fromJson(Map<String, dynamic> json) => ContractPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ContractDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class ContractExpiringPagedResultDto {
  const ContractExpiringPagedResultDto({
    required this.totalCount,
    required this.items,
    required this.alertDays,
  });

  final int totalCount;
  final List<ContractDto> items;
  /// القيمة الفعلية لمدة التنبيه المستخدمة في هذه النتيجة.
  final int alertDays;

  factory ContractExpiringPagedResultDto.fromJson(Map<String, dynamic> json) => ContractExpiringPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ContractDto.fromJson((e) as Map<String, dynamic>)).toList(),
        alertDays: ((json['alertDays']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
        'alertDays': alertDays,
      };
}

class ContractRecommendInputDto {
  const ContractRecommendInputDto({
    required this.employeePosition,
    required this.recommendation,
    this.note,
  });

  final String employeePosition;
  final ContractRenewalOption recommendation;
  final String? note;

  factory ContractRecommendInputDto.fromJson(Map<String, dynamic> json) => ContractRecommendInputDto(
        employeePosition: (json['employeePosition']) as String,
        recommendation: ContractRenewalOption.fromJson(json['recommendation']),
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employeePosition': employeePosition,
        'recommendation': recommendation.toJson(),
        if (note != null) 'note': note!,
      };
}

class ContractDecideInputDto {
  const ContractDecideInputDto({
    required this.decision,
    this.note,
    this.employeeMessage,
  });

  final ContractRenewalOption decision;
  /// ملاحظة داخلية لا تصل إلى الموظف.
  final String? note;
  /// نص اختياري يظهر للموظف.
  final String? employeeMessage;

  factory ContractDecideInputDto.fromJson(Map<String, dynamic> json) => ContractDecideInputDto(
        decision: ContractRenewalOption.fromJson(json['decision']),
        note: json['note'] == null ? null : (json['note']) as String,
        employeeMessage: json['employeeMessage'] == null ? null : (json['employeeMessage']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'decision': decision.toJson(),
        if (note != null) 'note': note!,
        if (employeeMessage != null) 'employeeMessage': employeeMessage!,
      };
}

/// عرض الموظف لعقده. لا يتضمن الموقف أو التوصية أو الملاحظة الداخلية أو هوية المعتمد.
class ContractEmployeeDto {
  const ContractEmployeeDto({
    required this.id,
    required this.contractType,
    required this.startDate,
    required this.endDate,
    required this.status,
    required this.isExpired,
    this.employeeMessage,
    this.decidedAt,
  });

  final String id;
  final String contractType;
  final DateTime startDate;
  final DateTime endDate;
  final ContractEmployeeStatus status;
  final bool isExpired;
  final String? employeeMessage;
  final DateTime? decidedAt;

  factory ContractEmployeeDto.fromJson(Map<String, dynamic> json) => ContractEmployeeDto(
        id: (json['id']) as String,
        contractType: (json['contractType']) as String,
        startDate: hrDateFromJson((json['startDate']) as String),
        endDate: hrDateFromJson((json['endDate']) as String),
        status: ContractEmployeeStatus.fromJson(json['status']),
        isExpired: (json['isExpired']) as bool,
        employeeMessage: json['employeeMessage'] == null ? null : (json['employeeMessage']) as String,
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse((json['decidedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'contractType': contractType,
        'startDate': hrDateToJson(startDate),
        'endDate': hrDateToJson(endDate),
        'status': status.toJson(),
        'isExpired': isExpired,
        'employeeMessage': employeeMessage == null ? null : employeeMessage!,
        'decidedAt': decidedAt == null ? null : hrDateTimeToJson(decidedAt!),
      };
}

class ContractEmployeePagedResultDto {
  const ContractEmployeePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ContractEmployeeDto> items;

  factory ContractEmployeePagedResultDto.fromJson(Map<String, dynamic> json) => ContractEmployeePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ContractEmployeeDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

enum WarningLevel {
  first('first'),
  final_('final'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WarningLevel(this.value);
  final String value;

  static WarningLevel fromJson(Object? json) {
    for (final v in WarningLevel.values) {
      if (v.value == json) return v;
    }
    return WarningLevel.$unknown;
  }

  String toJson() => value;
}

/// recommended: توصية مسجلة لم يُبلغ بها الموظف، ولا تعد إنذارًا. notified: أُبلغ الموظف ومهلة الرد مفتوحة. responded: رد الموظف. responseExpired: انتهت المهلة دون رد. issued: صدر الإنذار بقرار الموارد البشرية. dismissed: قررت الموارد البشرية عدم الإصدار. objected: اعترض الموظف. objectionUpheld: أيد المدير التنفيذي الإنذار. objectionOverturned: ألغى المدير التنفيذي الإنذار.
enum WarningStatus {
  recommended('recommended'),
  notified('notified'),
  responded('responded'),
  responseExpired('responseExpired'),
  issued('issued'),
  dismissed('dismissed'),
  objected('objected'),
  objectionUpheld('objectionUpheld'),
  objectionOverturned('objectionOverturned'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WarningStatus(this.value);
  final String value;

  static WarningStatus fromJson(Object? json) {
    for (final v in WarningStatus.values) {
      if (v.value == json) return v;
    }
    return WarningStatus.$unknown;
  }

  String toJson() => value;
}

enum WarningRecommenderRole {
  departmentManager('departmentManager'),
  reviewer('reviewer'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WarningRecommenderRole(this.value);
  final String value;

  static WarningRecommenderRole fromJson(Object? json) {
    for (final v in WarningRecommenderRole.values) {
      if (v.value == json) return v;
    }
    return WarningRecommenderRole.$unknown;
  }

  String toJson() => value;
}

enum WarningDecisionOutcome {
  issue('issue'),
  dismiss('dismiss'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WarningDecisionOutcome(this.value);
  final String value;

  static WarningDecisionOutcome fromJson(Object? json) {
    for (final v in WarningDecisionOutcome.values) {
      if (v.value == json) return v;
    }
    return WarningDecisionOutcome.$unknown;
  }

  String toJson() => value;
}

enum WarningObjectionOutcome {
  uphold('uphold'),
  overturn('overturn'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WarningObjectionOutcome(this.value);
  final String value;

  static WarningObjectionOutcome fromJson(Object? json) {
    for (final v in WarningObjectionOutcome.values) {
      if (v.value == json) return v;
    }
    return WarningObjectionOutcome.$unknown;
  }

  String toJson() => value;
}

/// المرحلة التي يراها صاحب التوصية، دون كشف الرد أو الاعتراض.
enum WarningRecommendationStage {
  underReview('underReview'),
  issued('issued'),
  notIssued('notIssued'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const WarningRecommendationStage(this.value);
  final String value;

  static WarningRecommendationStage fromJson(Object? json) {
    for (final v in WarningRecommendationStage.values) {
      if (v.value == json) return v;
    }
    return WarningRecommendationStage.$unknown;
  }

  String toJson() => value;
}

class WarningCreateInputDto {
  const WarningCreateInputDto({
    required this.employeeId,
    required this.recommendedLevel,
    required this.incidentDate,
    required this.description,
    this.attachments,
  });

  final String employeeId;
  final WarningLevel recommendedLevel;
  /// تاريخ الواقعة. لا يكون في المستقبل.
  final DateTime incidentDate;
  final String description;
  /// مستندات الواقعة.
  final List<MultipartFile>? attachments;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employeeId': employeeId,
        'recommendedLevel': recommendedLevel.toJson(),
        'incidentDate': hrDateToJson(incidentDate),
        'description': description,
      };

  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,
  /// one part per file for arrays (x-server-requirements.multipart).
  FormData toFormData() {
    final form = FormData();
    form.fields.add(MapEntry('employeeId', hrFormValue(employeeId)));
    form.fields.add(MapEntry('recommendedLevel', hrFormValue(recommendedLevel.toJson())));
    form.fields.add(MapEntry('incidentDate', hrFormValue(hrDateToJson(incidentDate))));
    form.fields.add(MapEntry('description', hrFormValue(description)));
    if (attachments != null) for (final file in attachments!) {
      form.files.add(MapEntry('attachments', file));
    }
    return form;
  }
}

class WarningResponseDto {
  const WarningResponseDto({
    required this.content,
    required this.submittedAt,
  });

  final String content;
  final DateTime submittedAt;

  factory WarningResponseDto.fromJson(Map<String, dynamic> json) => WarningResponseDto(
        content: (json['content']) as String,
        submittedAt: DateTime.parse((json['submittedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'content': content,
        'submittedAt': hrDateTimeToJson(submittedAt),
      };
}

class WarningDecisionDto {
  const WarningDecisionDto({
    required this.outcome,
    this.issuedLevel,
    required this.note,
    required this.decidedBy,
    required this.decidedAt,
  });

  final WarningDecisionOutcome outcome;
  final WarningLevel? issuedLevel;
  final String note;
  final UserRefDto decidedBy;
  final DateTime decidedAt;

  factory WarningDecisionDto.fromJson(Map<String, dynamic> json) => WarningDecisionDto(
        outcome: WarningDecisionOutcome.fromJson(json['outcome']),
        issuedLevel: json['issuedLevel'] == null ? null : WarningLevel.fromJson(json['issuedLevel']),
        note: (json['note']) as String,
        decidedBy: UserRefDto.fromJson((json['decidedBy']) as Map<String, dynamic>),
        decidedAt: DateTime.parse((json['decidedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'outcome': outcome.toJson(),
        'issuedLevel': issuedLevel == null ? null : issuedLevel!.toJson(),
        'note': note,
        'decidedBy': decidedBy.toJson(),
        'decidedAt': hrDateTimeToJson(decidedAt),
      };
}

class WarningObjectionDto {
  const WarningObjectionDto({
    required this.content,
    required this.submittedAt,
    this.outcome,
    this.resolutionNote,
    this.resolvedBy,
    this.resolvedAt,
  });

  final String content;
  final DateTime submittedAt;
  final WarningObjectionOutcome? outcome;
  final String? resolutionNote;
  final UserRefDto? resolvedBy;
  final DateTime? resolvedAt;

  factory WarningObjectionDto.fromJson(Map<String, dynamic> json) => WarningObjectionDto(
        content: (json['content']) as String,
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        outcome: json['outcome'] == null ? null : WarningObjectionOutcome.fromJson(json['outcome']),
        resolutionNote: json['resolutionNote'] == null ? null : (json['resolutionNote']) as String,
        resolvedBy: json['resolvedBy'] == null ? null : UserRefDto.fromJson((json['resolvedBy']) as Map<String, dynamic>),
        resolvedAt: json['resolvedAt'] == null ? null : DateTime.parse((json['resolvedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'content': content,
        'submittedAt': hrDateTimeToJson(submittedAt),
        'outcome': outcome == null ? null : outcome!.toJson(),
        'resolutionNote': resolutionNote == null ? null : resolutionNote!,
        'resolvedBy': resolvedBy == null ? null : resolvedBy!.toJson(),
        'resolvedAt': resolvedAt == null ? null : hrDateTimeToJson(resolvedAt!),
      };
}

/// العرض الكامل للموارد البشرية والمدير التنفيذي.
class WarningDto {
  const WarningDto({
    required this.id,
    required this.employee,
    required this.recommendedLevel,
    required this.incidentDate,
    required this.description,
    required this.attachments,
    required this.recommendedBy,
    required this.recommenderRole,
    required this.status,
    this.notifiedAt,
    this.responseDueDate,
    this.employeeResponse,
    this.decision,
    this.objection,
    required this.creationTime,
  });

  final String id;
  final EmployeeRefDto employee;
  final WarningLevel recommendedLevel;
  final DateTime incidentDate;
  final String description;
  final List<AttachmentDto> attachments;
  final UserRefDto recommendedBy;
  final WarningRecommenderRole recommenderRole;
  final WarningStatus status;
  final DateTime? notifiedAt;
  /// 3 أيام دوام من يوم الإبلاغ.
  final DateTime? responseDueDate;
  final WarningResponseDto? employeeResponse;
  final WarningDecisionDto? decision;
  final WarningObjectionDto? objection;
  final DateTime creationTime;

  factory WarningDto.fromJson(Map<String, dynamic> json) => WarningDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        recommendedLevel: WarningLevel.fromJson(json['recommendedLevel']),
        incidentDate: hrDateFromJson((json['incidentDate']) as String),
        description: (json['description']) as String,
        attachments: ((json['attachments']) as List).map((e) => AttachmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
        recommendedBy: UserRefDto.fromJson((json['recommendedBy']) as Map<String, dynamic>),
        recommenderRole: WarningRecommenderRole.fromJson(json['recommenderRole']),
        status: WarningStatus.fromJson(json['status']),
        notifiedAt: json['notifiedAt'] == null ? null : DateTime.parse((json['notifiedAt']) as String),
        responseDueDate: json['responseDueDate'] == null ? null : hrDateFromJson((json['responseDueDate']) as String),
        employeeResponse: json['employeeResponse'] == null ? null : WarningResponseDto.fromJson((json['employeeResponse']) as Map<String, dynamic>),
        decision: json['decision'] == null ? null : WarningDecisionDto.fromJson((json['decision']) as Map<String, dynamic>),
        objection: json['objection'] == null ? null : WarningObjectionDto.fromJson((json['objection']) as Map<String, dynamic>),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'recommendedLevel': recommendedLevel.toJson(),
        'incidentDate': hrDateToJson(incidentDate),
        'description': description,
        'attachments': attachments.map((e) => e.toJson()).toList(),
        'recommendedBy': recommendedBy.toJson(),
        'recommenderRole': recommenderRole.toJson(),
        'status': status.toJson(),
        'notifiedAt': notifiedAt == null ? null : hrDateTimeToJson(notifiedAt!),
        'responseDueDate': responseDueDate == null ? null : hrDateToJson(responseDueDate!),
        'employeeResponse': employeeResponse == null ? null : employeeResponse!.toJson(),
        'decision': decision == null ? null : decision!.toJson(),
        'objection': objection == null ? null : objection!.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class WarningPagedResultDto {
  const WarningPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<WarningDto> items;

  factory WarningPagedResultDto.fromJson(Map<String, dynamic> json) => WarningPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => WarningDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class WarningEmployeeObjectionDto {
  const WarningEmployeeObjectionDto({
    required this.content,
    required this.submittedAt,
    this.outcome,
    this.resolutionNote,
    this.resolvedAt,
  });

  final String content;
  final DateTime submittedAt;
  final WarningObjectionOutcome? outcome;
  final String? resolutionNote;
  final DateTime? resolvedAt;

  factory WarningEmployeeObjectionDto.fromJson(Map<String, dynamic> json) => WarningEmployeeObjectionDto(
        content: (json['content']) as String,
        submittedAt: DateTime.parse((json['submittedAt']) as String),
        outcome: json['outcome'] == null ? null : WarningObjectionOutcome.fromJson(json['outcome']),
        resolutionNote: json['resolutionNote'] == null ? null : (json['resolutionNote']) as String,
        resolvedAt: json['resolvedAt'] == null ? null : DateTime.parse((json['resolvedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'content': content,
        'submittedAt': hrDateTimeToJson(submittedAt),
        'outcome': outcome == null ? null : outcome!.toJson(),
        'resolutionNote': resolutionNote == null ? null : resolutionNote!,
        'resolvedAt': resolvedAt == null ? null : hrDateTimeToJson(resolvedAt!),
      };
}

/// عرض الموظف. لا يتضمن صاحب التوصية أو مستنداتها أو هوية المعتمد.
class WarningEmployeeDto {
  const WarningEmployeeDto({
    required this.id,
    required this.level,
    required this.incidentDate,
    required this.description,
    required this.status,
    required this.notifiedAt,
    required this.responseDueDate,
    required this.canRespond,
    required this.canObject,
    this.myResponse,
    this.decisionNote,
    this.decidedAt,
    this.myObjection,
  });

  final String id;
  /// الدرجة الصادرة بعد القرار، والدرجة الموصى بها قبله.
  final WarningLevel level;
  final DateTime incidentDate;
  final String description;
  /// لا تكون recommended في هذا العرض.
  final WarningStatus status;
  final DateTime notifiedAt;
  final DateTime responseDueDate;
  final bool canRespond;
  final bool canObject;
  final WarningResponseDto? myResponse;
  final String? decisionNote;
  final DateTime? decidedAt;
  final WarningEmployeeObjectionDto? myObjection;

  factory WarningEmployeeDto.fromJson(Map<String, dynamic> json) => WarningEmployeeDto(
        id: (json['id']) as String,
        level: WarningLevel.fromJson(json['level']),
        incidentDate: hrDateFromJson((json['incidentDate']) as String),
        description: (json['description']) as String,
        status: WarningStatus.fromJson(json['status']),
        notifiedAt: DateTime.parse((json['notifiedAt']) as String),
        responseDueDate: hrDateFromJson((json['responseDueDate']) as String),
        canRespond: (json['canRespond']) as bool,
        canObject: (json['canObject']) as bool,
        myResponse: json['myResponse'] == null ? null : WarningResponseDto.fromJson((json['myResponse']) as Map<String, dynamic>),
        decisionNote: json['decisionNote'] == null ? null : (json['decisionNote']) as String,
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse((json['decidedAt']) as String),
        myObjection: json['myObjection'] == null ? null : WarningEmployeeObjectionDto.fromJson((json['myObjection']) as Map<String, dynamic>),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'level': level.toJson(),
        'incidentDate': hrDateToJson(incidentDate),
        'description': description,
        'status': status.toJson(),
        'notifiedAt': hrDateTimeToJson(notifiedAt),
        'responseDueDate': hrDateToJson(responseDueDate),
        'canRespond': canRespond,
        'canObject': canObject,
        'myResponse': myResponse == null ? null : myResponse!.toJson(),
        'decisionNote': decisionNote == null ? null : decisionNote!,
        'decidedAt': decidedAt == null ? null : hrDateTimeToJson(decidedAt!),
        'myObjection': myObjection == null ? null : myObjection!.toJson(),
      };
}

class WarningEmployeePagedResultDto {
  const WarningEmployeePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<WarningEmployeeDto> items;

  factory WarningEmployeePagedResultDto.fromJson(Map<String, dynamic> json) => WarningEmployeePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => WarningEmployeeDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// عرض صاحب التوصية. لا يتضمن رد الموظف أو اعتراضه أو ملاحظات القرار.
class WarningRecommendationDto {
  const WarningRecommendationDto({
    required this.id,
    required this.employee,
    required this.recommendedLevel,
    required this.incidentDate,
    required this.description,
    required this.stage,
    this.issuedLevel,
    this.decidedAt,
    required this.creationTime,
  });

  final String id;
  final EmployeeRefDto employee;
  final WarningLevel recommendedLevel;
  final DateTime incidentDate;
  final String description;
  final WarningRecommendationStage stage;
  final WarningLevel? issuedLevel;
  final DateTime? decidedAt;
  final DateTime creationTime;

  factory WarningRecommendationDto.fromJson(Map<String, dynamic> json) => WarningRecommendationDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        recommendedLevel: WarningLevel.fromJson(json['recommendedLevel']),
        incidentDate: hrDateFromJson((json['incidentDate']) as String),
        description: (json['description']) as String,
        stage: WarningRecommendationStage.fromJson(json['stage']),
        issuedLevel: json['issuedLevel'] == null ? null : WarningLevel.fromJson(json['issuedLevel']),
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse((json['decidedAt']) as String),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'recommendedLevel': recommendedLevel.toJson(),
        'incidentDate': hrDateToJson(incidentDate),
        'description': description,
        'stage': stage.toJson(),
        'issuedLevel': issuedLevel == null ? null : issuedLevel!.toJson(),
        'decidedAt': decidedAt == null ? null : hrDateTimeToJson(decidedAt!),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class WarningRecommendationPagedResultDto {
  const WarningRecommendationPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<WarningRecommendationDto> items;

  factory WarningRecommendationPagedResultDto.fromJson(Map<String, dynamic> json) => WarningRecommendationPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => WarningRecommendationDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// سجل الإنذار في ملف الموظف: القرار والرد.
class WarningFileEntryDto {
  const WarningFileEntryDto({
    required this.id,
    required this.level,
    required this.incidentDate,
    required this.description,
    required this.status,
    required this.decisionNote,
    required this.decidedAt,
    this.employeeResponse,
    this.objectionOutcome,
  });

  final String id;
  /// الدرجة الصادرة، أو الموصى بها إذا لم يصدر إنذار.
  final WarningLevel level;
  final DateTime incidentDate;
  final String description;
  final WarningStatus status;
  final String decisionNote;
  final DateTime decidedAt;
  final WarningResponseDto? employeeResponse;
  final WarningObjectionOutcome? objectionOutcome;

  factory WarningFileEntryDto.fromJson(Map<String, dynamic> json) => WarningFileEntryDto(
        id: (json['id']) as String,
        level: WarningLevel.fromJson(json['level']),
        incidentDate: hrDateFromJson((json['incidentDate']) as String),
        description: (json['description']) as String,
        status: WarningStatus.fromJson(json['status']),
        decisionNote: (json['decisionNote']) as String,
        decidedAt: DateTime.parse((json['decidedAt']) as String),
        employeeResponse: json['employeeResponse'] == null ? null : WarningResponseDto.fromJson((json['employeeResponse']) as Map<String, dynamic>),
        objectionOutcome: json['objectionOutcome'] == null ? null : WarningObjectionOutcome.fromJson(json['objectionOutcome']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'level': level.toJson(),
        'incidentDate': hrDateToJson(incidentDate),
        'description': description,
        'status': status.toJson(),
        'decisionNote': decisionNote,
        'decidedAt': hrDateTimeToJson(decidedAt),
        'employeeResponse': employeeResponse == null ? null : employeeResponse!.toJson(),
        'objectionOutcome': objectionOutcome == null ? null : objectionOutcome!.toJson(),
      };
}

class WarningFileEntryPagedResultDto {
  const WarningFileEntryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<WarningFileEntryDto> items;

  factory WarningFileEntryPagedResultDto.fromJson(Map<String, dynamic> json) => WarningFileEntryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => WarningFileEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class WarningRespondInputDto {
  const WarningRespondInputDto({
    required this.content,
  });

  final String content;

  factory WarningRespondInputDto.fromJson(Map<String, dynamic> json) => WarningRespondInputDto(
        content: (json['content']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'content': content,
      };
}

class WarningDecideInputDto {
  const WarningDecideInputDto({
    required this.decision,
    this.issuedLevel,
    required this.note,
  });

  final WarningDecisionOutcome decision;
  /// مطلوبة عند issue، ويجب أن تكون فارغة عند dismiss. يمكن أن تختلف عن الدرجة الموصى بها.
  final WarningLevel? issuedLevel;
  final String note;

  factory WarningDecideInputDto.fromJson(Map<String, dynamic> json) => WarningDecideInputDto(
        decision: WarningDecisionOutcome.fromJson(json['decision']),
        issuedLevel: json['issuedLevel'] == null ? null : WarningLevel.fromJson(json['issuedLevel']),
        note: (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'decision': decision.toJson(),
        if (issuedLevel != null) 'issuedLevel': issuedLevel!.toJson(),
        'note': note,
      };
}

class WarningObjectInputDto {
  const WarningObjectInputDto({
    required this.content,
  });

  final String content;

  factory WarningObjectInputDto.fromJson(Map<String, dynamic> json) => WarningObjectInputDto(
        content: (json['content']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'content': content,
      };
}

class WarningResolveObjectionInputDto {
  const WarningResolveObjectionInputDto({
    required this.outcome,
    required this.note,
  });

  final WarningObjectionOutcome outcome;
  final String note;

  factory WarningResolveObjectionInputDto.fromJson(Map<String, dynamic> json) => WarningResolveObjectionInputDto(
        outcome: WarningObjectionOutcome.fromJson(json['outcome']),
        note: (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'outcome': outcome.toJson(),
        'note': note,
      };
}

class AppreciationLetterCreateInputDto {
  const AppreciationLetterCreateInputDto({
    required this.employeeId,
    required this.title,
    this.note,
    required this.file,
  });

  final String employeeId;
  final String title;
  final String? note;
  /// ملف كتاب الشكر.
  final MultipartFile file;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employeeId': employeeId,
        'title': title,
        if (note != null) 'note': note!,
      };

  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,
  /// one part per file for arrays (x-server-requirements.multipart).
  FormData toFormData() {
    final form = FormData();
    form.fields.add(MapEntry('employeeId', hrFormValue(employeeId)));
    form.fields.add(MapEntry('title', hrFormValue(title)));
    if (note != null) form.fields.add(MapEntry('note', hrFormValue(note!)));
    form.files.add(MapEntry('file', file));
    return form;
  }
}

class AppreciationLetterDto {
  const AppreciationLetterDto({
    required this.id,
    required this.employee,
    required this.title,
    this.note,
    required this.issuedBy,
    required this.issuedAt,
    required this.file,
    required this.isAcknowledged,
    this.acknowledgedAt,
  });

  final String id;
  final EmployeeRefDto employee;
  final String title;
  final String? note;
  final UserRefDto issuedBy;
  final DateTime issuedAt;
  final AttachmentDto file;
  final bool isAcknowledged;
  final DateTime? acknowledgedAt;

  factory AppreciationLetterDto.fromJson(Map<String, dynamic> json) => AppreciationLetterDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        title: (json['title']) as String,
        note: json['note'] == null ? null : (json['note']) as String,
        issuedBy: UserRefDto.fromJson((json['issuedBy']) as Map<String, dynamic>),
        issuedAt: DateTime.parse((json['issuedAt']) as String),
        file: AttachmentDto.fromJson((json['file']) as Map<String, dynamic>),
        isAcknowledged: (json['isAcknowledged']) as bool,
        acknowledgedAt: json['acknowledgedAt'] == null ? null : DateTime.parse((json['acknowledgedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'title': title,
        'note': note == null ? null : note!,
        'issuedBy': issuedBy.toJson(),
        'issuedAt': hrDateTimeToJson(issuedAt),
        'file': file.toJson(),
        'isAcknowledged': isAcknowledged,
        'acknowledgedAt': acknowledgedAt == null ? null : hrDateTimeToJson(acknowledgedAt!),
      };
}

class AppreciationLetterPagedResultDto {
  const AppreciationLetterPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<AppreciationLetterDto> items;

  factory AppreciationLetterPagedResultDto.fromJson(Map<String, dynamic> json) => AppreciationLetterPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AppreciationLetterDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

enum SalaryComponentType {
  allowance('allowance'),
  fixedDeduction('fixedDeduction'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const SalaryComponentType(this.value);
  final String value;

  static SalaryComponentType fromJson(Object? json) {
    for (final v in SalaryComponentType.values) {
      if (v.value == json) return v;
    }
    return SalaryComponentType.$unknown;
  }

  String toJson() => value;
}

class SalaryComponentInputDto {
  const SalaryComponentInputDto({
    required this.name,
    required this.type,
    required this.amount,
  });

  final String name;
  final SalaryComponentType type;
  final double amount;

  factory SalaryComponentInputDto.fromJson(Map<String, dynamic> json) => SalaryComponentInputDto(
        name: (json['name']) as String,
        type: SalaryComponentType.fromJson(json['type']),
        amount: ((json['amount']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'name': name,
        'type': type.toJson(),
        'amount': amount,
      };
}

class SalaryComponentDto {
  const SalaryComponentDto({
    required this.id,
    required this.name,
    required this.type,
    required this.amount,
  });

  final String id;
  final String name;
  final SalaryComponentType type;
  final double amount;

  factory SalaryComponentDto.fromJson(Map<String, dynamic> json) => SalaryComponentDto(
        id: (json['id']) as String,
        name: (json['name']) as String,
        type: SalaryComponentType.fromJson(json['type']),
        amount: ((json['amount']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'name': name,
        'type': type.toJson(),
        'amount': amount,
      };
}

class SalaryProfileDto {
  const SalaryProfileDto({
    required this.id,
    required this.employee,
    required this.baseSalary,
    required this.currency,
    required this.effectiveFrom,
    required this.components,
    required this.concurrencyStamp,
    required this.creationTime,
  });

  final String id;
  final EmployeeRefDto employee;
  final double baseSalary;
  /// ISO 4217.
  final String currency;
  final DateTime effectiveFrom;
  final List<SalaryComponentDto> components;
  final String concurrencyStamp;
  final DateTime creationTime;

  factory SalaryProfileDto.fromJson(Map<String, dynamic> json) => SalaryProfileDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        baseSalary: ((json['baseSalary']) as num).toDouble(),
        currency: (json['currency']) as String,
        effectiveFrom: hrDateFromJson((json['effectiveFrom']) as String),
        components: ((json['components']) as List).map((e) => SalaryComponentDto.fromJson((e) as Map<String, dynamic>)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'baseSalary': baseSalary,
        'currency': currency,
        'effectiveFrom': hrDateToJson(effectiveFrom),
        'components': components.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class SalaryProfilePagedResultDto {
  const SalaryProfilePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<SalaryProfileDto> items;

  factory SalaryProfilePagedResultDto.fromJson(Map<String, dynamic> json) => SalaryProfilePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => SalaryProfileDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class SalaryProfileListItemDto {
  const SalaryProfileListItemDto({
    required this.employee,
    required this.hasProfile,
    this.baseSalary,
    this.currency,
    this.effectiveFrom,
  });

  final EmployeeRefDto employee;
  final bool hasProfile;
  final double? baseSalary;
  /// ISO 4217.
  final String? currency;
  final DateTime? effectiveFrom;

  factory SalaryProfileListItemDto.fromJson(Map<String, dynamic> json) => SalaryProfileListItemDto(
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        hasProfile: (json['hasProfile']) as bool,
        baseSalary: json['baseSalary'] == null ? null : ((json['baseSalary']) as num).toDouble(),
        currency: json['currency'] == null ? null : (json['currency']) as String,
        effectiveFrom: json['effectiveFrom'] == null ? null : hrDateFromJson((json['effectiveFrom']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employee': employee.toJson(),
        'hasProfile': hasProfile,
        'baseSalary': baseSalary == null ? null : baseSalary!,
        'currency': currency == null ? null : currency!,
        'effectiveFrom': effectiveFrom == null ? null : hrDateToJson(effectiveFrom!),
      };
}

class SalaryProfileListItemPagedResultDto {
  const SalaryProfileListItemPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<SalaryProfileListItemDto> items;

  factory SalaryProfileListItemPagedResultDto.fromJson(Map<String, dynamic> json) => SalaryProfileListItemPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => SalaryProfileListItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class SalaryProfileUpdateInputDto {
  const SalaryProfileUpdateInputDto({
    required this.baseSalary,
    required this.currency,
    required this.effectiveFrom,
    required this.components,
    this.concurrencyStamp,
  });

  final double baseSalary;
  /// ISO 4217.
  final String currency;
  /// بعد تاريخ آخر نسخة، وخارج أي فترة لها كشف معتمد أو مغلق.
  final DateTime effectiveFrom;
  final List<SalaryComponentInputDto> components;
  /// قيمة آخر قراءة، وnull عند إنشاء أول نسخة للموظف.
  final String? concurrencyStamp;

  factory SalaryProfileUpdateInputDto.fromJson(Map<String, dynamic> json) => SalaryProfileUpdateInputDto(
        baseSalary: ((json['baseSalary']) as num).toDouble(),
        currency: (json['currency']) as String,
        effectiveFrom: hrDateFromJson((json['effectiveFrom']) as String),
        components: ((json['components']) as List).map((e) => SalaryComponentInputDto.fromJson((e) as Map<String, dynamic>)).toList(),
        concurrencyStamp: json['concurrencyStamp'] == null ? null : (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'baseSalary': baseSalary,
        'currency': currency,
        'effectiveFrom': hrDateToJson(effectiveFrom),
        'components': components.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp == null ? null : concurrencyStamp!,
      };
}

enum AdjustmentType {
  addition('addition'),
  deduction('deduction'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AdjustmentType(this.value);
  final String value;

  static AdjustmentType fromJson(Object? json) {
    for (final v in AdjustmentType.values) {
      if (v.value == json) return v;
    }
    return AdjustmentType.$unknown;
  }

  String toJson() => value;
}

enum AdjustmentSource {
  manual('manual'),
  attendanceSuggestion('attendanceSuggestion'),
  reversal('reversal'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AdjustmentSource(this.value);
  final String value;

  static AdjustmentSource fromJson(Object? json) {
    for (final v in AdjustmentSource.values) {
      if (v.value == json) return v;
    }
    return AdjustmentSource.$unknown;
  }

  String toJson() => value;
}

/// open: يدخل في أي كشف مسودة للفترة. includedInRun: ثبت في كشف معتمد أو مغلق. reversed: عُكس بحركة مقابلة.
enum AdjustmentStatus {
  open('open'),
  includedInRun('includedInRun'),
  reversed('reversed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AdjustmentStatus(this.value);
  final String value;

  static AdjustmentStatus fromJson(Object? json) {
    for (final v in AdjustmentStatus.values) {
      if (v.value == json) return v;
    }
    return AdjustmentStatus.$unknown;
  }

  String toJson() => value;
}

class AdjustmentCreateInputDto {
  const AdjustmentCreateInputDto({
    required this.employeeId,
    required this.period,
    required this.type,
    required this.amount,
    required this.reason,
    required this.attachment,
    this.sourceRequestId,
  });

  final String employeeId;
  final String period;
  final AdjustmentType type;
  final double amount;
  final String reason;
  /// مرفق واحد.
  final MultipartFile attachment;
  /// طلب المحاسب المرتبط، إن وجد.
  final String? sourceRequestId;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'employeeId': employeeId,
        'period': period,
        'type': type.toJson(),
        'amount': amount,
        'reason': reason,
        if (sourceRequestId != null) 'sourceRequestId': sourceRequestId!,
      };

  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,
  /// one part per file for arrays (x-server-requirements.multipart).
  FormData toFormData() {
    final form = FormData();
    form.fields.add(MapEntry('employeeId', hrFormValue(employeeId)));
    form.fields.add(MapEntry('period', hrFormValue(period)));
    form.fields.add(MapEntry('type', hrFormValue(type.toJson())));
    form.fields.add(MapEntry('amount', hrFormValue(amount)));
    form.fields.add(MapEntry('reason', hrFormValue(reason)));
    form.files.add(MapEntry('attachment', attachment));
    if (sourceRequestId != null) form.fields.add(MapEntry('sourceRequestId', hrFormValue(sourceRequestId!)));
    return form;
  }
}

class AdjustmentDto {
  const AdjustmentDto({
    required this.id,
    required this.employee,
    required this.period,
    required this.type,
    required this.amount,
    required this.currency,
    required this.reason,
    required this.source,
    required this.status,
    this.attachment,
    this.sourceRequestId,
    this.suggestionId,
    this.reversalOfAdjustmentId,
    required this.createdBy,
    required this.creationTime,
  });

  final String id;
  final EmployeeRefDto employee;
  final String period;
  final AdjustmentType type;
  final double amount;
  /// ISO 4217.
  final String currency;
  final String reason;
  final AdjustmentSource source;
  final AdjustmentStatus status;
  final AttachmentDto? attachment;
  final String? sourceRequestId;
  final String? suggestionId;
  final String? reversalOfAdjustmentId;
  final UserRefDto createdBy;
  final DateTime creationTime;

  factory AdjustmentDto.fromJson(Map<String, dynamic> json) => AdjustmentDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        period: (json['period']) as String,
        type: AdjustmentType.fromJson(json['type']),
        amount: ((json['amount']) as num).toDouble(),
        currency: (json['currency']) as String,
        reason: (json['reason']) as String,
        source: AdjustmentSource.fromJson(json['source']),
        status: AdjustmentStatus.fromJson(json['status']),
        attachment: json['attachment'] == null ? null : AttachmentDto.fromJson((json['attachment']) as Map<String, dynamic>),
        sourceRequestId: json['sourceRequestId'] == null ? null : (json['sourceRequestId']) as String,
        suggestionId: json['suggestionId'] == null ? null : (json['suggestionId']) as String,
        reversalOfAdjustmentId: json['reversalOfAdjustmentId'] == null ? null : (json['reversalOfAdjustmentId']) as String,
        createdBy: UserRefDto.fromJson((json['createdBy']) as Map<String, dynamic>),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'period': period,
        'type': type.toJson(),
        'amount': amount,
        'currency': currency,
        'reason': reason,
        'source': source.toJson(),
        'status': status.toJson(),
        'attachment': attachment == null ? null : attachment!.toJson(),
        'sourceRequestId': sourceRequestId == null ? null : sourceRequestId!,
        'suggestionId': suggestionId == null ? null : suggestionId!,
        'reversalOfAdjustmentId': reversalOfAdjustmentId == null ? null : reversalOfAdjustmentId!,
        'createdBy': createdBy.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class AdjustmentPagedResultDto {
  const AdjustmentPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<AdjustmentDto> items;

  factory AdjustmentPagedResultDto.fromJson(Map<String, dynamic> json) => AdjustmentPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AdjustmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class AdjustmentReverseInputDto {
  const AdjustmentReverseInputDto({
    required this.reason,
    required this.period,
  });

  final String reason;
  /// فترة الحركة العكسية. يجب ألا يكون لها كشف معتمد أو مغلق.
  final String period;

  factory AdjustmentReverseInputDto.fromJson(Map<String, dynamic> json) => AdjustmentReverseInputDto(
        reason: (json['reason']) as String,
        period: (json['period']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'reason': reason,
        'period': period,
      };
}

enum AdjustmentSuggestionBasis {
  lateness('lateness'),
  absence('absence'),
  overtime('overtime'),
  attendanceCorrectionSettlement('attendanceCorrectionSettlement'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AdjustmentSuggestionBasis(this.value);
  final String value;

  static AdjustmentSuggestionBasis fromJson(Object? json) {
    for (final v in AdjustmentSuggestionBasis.values) {
      if (v.value == json) return v;
    }
    return AdjustmentSuggestionBasis.$unknown;
  }

  String toJson() => value;
}

enum AdjustmentSuggestionQuantityUnit {
  minutes('minutes'),
  hours('hours'),
  days('days'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AdjustmentSuggestionQuantityUnit(this.value);
  final String value;

  static AdjustmentSuggestionQuantityUnit fromJson(Object? json) {
    for (final v in AdjustmentSuggestionQuantityUnit.values) {
      if (v.value == json) return v;
    }
    return AdjustmentSuggestionQuantityUnit.$unknown;
  }

  String toJson() => value;
}

enum AdjustmentSuggestionStatus {
  pendingReview('pendingReview'),
  approved('approved'),
  rejected('rejected'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AdjustmentSuggestionStatus(this.value);
  final String value;

  static AdjustmentSuggestionStatus fromJson(Object? json) {
    for (final v in AdjustmentSuggestionStatus.values) {
      if (v.value == json) return v;
    }
    return AdjustmentSuggestionStatus.$unknown;
  }

  String toJson() => value;
}

class AdjustmentSuggestionDto {
  const AdjustmentSuggestionDto({
    required this.id,
    required this.employee,
    required this.period,
    required this.type,
    required this.basis,
    required this.quantity,
    required this.quantityUnit,
    this.suggestedAmount,
    this.currency,
    this.settlementForPeriod,
    required this.status,
    this.adjustmentId,
    this.reviewedBy,
    this.reviewedAt,
    this.reviewNote,
    required this.creationTime,
  });

  final String id;
  final EmployeeRefDto employee;
  final String period;
  final AdjustmentType type;
  final AdjustmentSuggestionBasis basis;
  final double quantity;
  final AdjustmentSuggestionQuantityUnit quantityUnit;
  /// null إذا لم يتمكن النظام من الحساب، وعندها يدخل المحاسب المبلغ.
  final double? suggestedAmount;
  /// ISO 4217.
  final String? currency;
  /// الشهر المغلق الذي تعالجه التسوية عند تصحيح الحضور بعد الإغلاق.
  final String? settlementForPeriod;
  final AdjustmentSuggestionStatus status;
  final String? adjustmentId;
  final UserRefDto? reviewedBy;
  final DateTime? reviewedAt;
  final String? reviewNote;
  final DateTime creationTime;

  factory AdjustmentSuggestionDto.fromJson(Map<String, dynamic> json) => AdjustmentSuggestionDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        period: (json['period']) as String,
        type: AdjustmentType.fromJson(json['type']),
        basis: AdjustmentSuggestionBasis.fromJson(json['basis']),
        quantity: ((json['quantity']) as num).toDouble(),
        quantityUnit: AdjustmentSuggestionQuantityUnit.fromJson(json['quantityUnit']),
        suggestedAmount: json['suggestedAmount'] == null ? null : ((json['suggestedAmount']) as num).toDouble(),
        currency: json['currency'] == null ? null : (json['currency']) as String,
        settlementForPeriod: json['settlementForPeriod'] == null ? null : (json['settlementForPeriod']) as String,
        status: AdjustmentSuggestionStatus.fromJson(json['status']),
        adjustmentId: json['adjustmentId'] == null ? null : (json['adjustmentId']) as String,
        reviewedBy: json['reviewedBy'] == null ? null : UserRefDto.fromJson((json['reviewedBy']) as Map<String, dynamic>),
        reviewedAt: json['reviewedAt'] == null ? null : DateTime.parse((json['reviewedAt']) as String),
        reviewNote: json['reviewNote'] == null ? null : (json['reviewNote']) as String,
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'period': period,
        'type': type.toJson(),
        'basis': basis.toJson(),
        'quantity': quantity,
        'quantityUnit': quantityUnit.toJson(),
        'suggestedAmount': suggestedAmount == null ? null : suggestedAmount!,
        'currency': currency == null ? null : currency!,
        'settlementForPeriod': settlementForPeriod == null ? null : settlementForPeriod!,
        'status': status.toJson(),
        'adjustmentId': adjustmentId == null ? null : adjustmentId!,
        'reviewedBy': reviewedBy == null ? null : reviewedBy!.toJson(),
        'reviewedAt': reviewedAt == null ? null : hrDateTimeToJson(reviewedAt!),
        'reviewNote': reviewNote == null ? null : reviewNote!,
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class AdjustmentSuggestionPagedResultDto {
  const AdjustmentSuggestionPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<AdjustmentSuggestionDto> items;

  factory AdjustmentSuggestionPagedResultDto.fromJson(Map<String, dynamic> json) => AdjustmentSuggestionPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AdjustmentSuggestionDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class AdjustmentSuggestionApproveInputDto {
  const AdjustmentSuggestionApproveInputDto({
    this.amount,
    this.reason,
  });

  /// تعديل المبلغ. مطلوب إذا كان suggestedAmount فارغًا.
  final double? amount;
  /// يولد النظام سببًا من بيانات الحضور إذا تُرك فارغًا.
  final String? reason;

  factory AdjustmentSuggestionApproveInputDto.fromJson(Map<String, dynamic> json) => AdjustmentSuggestionApproveInputDto(
        amount: json['amount'] == null ? null : ((json['amount']) as num).toDouble(),
        reason: json['reason'] == null ? null : (json['reason']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        if (amount != null) 'amount': amount!,
        if (reason != null) 'reason': reason!,
      };
}

enum PayrollRunStatus {
  draft('draft'),
  approved('approved'),
  closed('closed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const PayrollRunStatus(this.value);
  final String value;

  static PayrollRunStatus fromJson(Object? json) {
    for (final v in PayrollRunStatus.values) {
      if (v.value == json) return v;
    }
    return PayrollRunStatus.$unknown;
  }

  String toJson() => value;
}

enum PayrollLineKind {
  baseSalary('baseSalary'),
  allowance('allowance'),
  fixedDeduction('fixedDeduction'),
  addition('addition'),
  deduction('deduction'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const PayrollLineKind(this.value);
  final String value;

  static PayrollLineKind fromJson(Object? json) {
    for (final v in PayrollLineKind.values) {
      if (v.value == json) return v;
    }
    return PayrollLineKind.$unknown;
  }

  String toJson() => value;
}

class PayrollLineDto {
  const PayrollLineDto({
    required this.kind,
    required this.label,
    required this.amount,
    this.adjustmentId,
  });

  final PayrollLineKind kind;
  final String label;
  final double amount;
  final String? adjustmentId;

  factory PayrollLineDto.fromJson(Map<String, dynamic> json) => PayrollLineDto(
        kind: PayrollLineKind.fromJson(json['kind']),
        label: (json['label']) as String,
        amount: ((json['amount']) as num).toDouble(),
        adjustmentId: json['adjustmentId'] == null ? null : (json['adjustmentId']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'kind': kind.toJson(),
        'label': label,
        'amount': amount,
        'adjustmentId': adjustmentId == null ? null : adjustmentId!,
      };
}

class PayrollItemDto {
  const PayrollItemDto({
    required this.id,
    required this.employee,
    required this.currency,
    required this.gross,
    required this.additions,
    required this.deductions,
    required this.net,
    required this.lines,
  });

  final String id;
  final EmployeeRefDto employee;
  /// ISO 4217.
  final String currency;
  /// الراتب الأساسي مع المخصصات الثابتة.
  final double gross;
  /// مجموع إضافات الفترة.
  final double additions;
  /// الاستقطاعات الثابتة مع استقطاعات الفترة.
  final double deductions;
  /// gross + additions - deductions.
  final double net;
  final List<PayrollLineDto> lines;

  factory PayrollItemDto.fromJson(Map<String, dynamic> json) => PayrollItemDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        currency: (json['currency']) as String,
        gross: ((json['gross']) as num).toDouble(),
        additions: ((json['additions']) as num).toDouble(),
        deductions: ((json['deductions']) as num).toDouble(),
        net: ((json['net']) as num).toDouble(),
        lines: ((json['lines']) as List).map((e) => PayrollLineDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'currency': currency,
        'gross': gross,
        'additions': additions,
        'deductions': deductions,
        'net': net,
        'lines': lines.map((e) => e.toJson()).toList(),
      };
}

class PayrollTotalDto {
  const PayrollTotalDto({
    required this.currency,
    required this.gross,
    required this.additions,
    required this.deductions,
    required this.net,
  });

  /// ISO 4217.
  final String currency;
  final double gross;
  final double additions;
  final double deductions;
  final double net;

  factory PayrollTotalDto.fromJson(Map<String, dynamic> json) => PayrollTotalDto(
        currency: (json['currency']) as String,
        gross: ((json['gross']) as num).toDouble(),
        additions: ((json['additions']) as num).toDouble(),
        deductions: ((json['deductions']) as num).toDouble(),
        net: ((json['net']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'currency': currency,
        'gross': gross,
        'additions': additions,
        'deductions': deductions,
        'net': net,
      };
}

class PayrollRunSummaryDto {
  const PayrollRunSummaryDto({
    required this.id,
    required this.period,
    required this.status,
    required this.preparedBy,
    this.approvalRequestedAt,
    this.approvedBy,
    this.approvedAt,
    this.closedAt,
    required this.lastCalculatedAt,
    required this.isStale,
    required this.employeeCount,
    required this.totals,
    required this.concurrencyStamp,
  });

  final String id;
  final String period;
  final PayrollRunStatus status;
  final UserRefDto preparedBy;
  /// وقت طلب الاعتماد. يعود null بعد أي إعادة حساب.
  final DateTime? approvalRequestedAt;
  final UserRefDto? approvedBy;
  final DateTime? approvedAt;
  final DateTime? closedAt;
  final DateTime lastCalculatedAt;
  /// true إذا تغيرت المدخلات بعد آخر حساب. يبقى false بعد الاعتماد.
  final bool isStale;
  final int employeeCount;
  final List<PayrollTotalDto> totals;
  final String concurrencyStamp;

  factory PayrollRunSummaryDto.fromJson(Map<String, dynamic> json) => PayrollRunSummaryDto(
        id: (json['id']) as String,
        period: (json['period']) as String,
        status: PayrollRunStatus.fromJson(json['status']),
        preparedBy: UserRefDto.fromJson((json['preparedBy']) as Map<String, dynamic>),
        approvalRequestedAt: json['approvalRequestedAt'] == null ? null : DateTime.parse((json['approvalRequestedAt']) as String),
        approvedBy: json['approvedBy'] == null ? null : UserRefDto.fromJson((json['approvedBy']) as Map<String, dynamic>),
        approvedAt: json['approvedAt'] == null ? null : DateTime.parse((json['approvedAt']) as String),
        closedAt: json['closedAt'] == null ? null : DateTime.parse((json['closedAt']) as String),
        lastCalculatedAt: DateTime.parse((json['lastCalculatedAt']) as String),
        isStale: (json['isStale']) as bool,
        employeeCount: ((json['employeeCount']) as num).toInt(),
        totals: ((json['totals']) as List).map((e) => PayrollTotalDto.fromJson((e) as Map<String, dynamic>)).toList(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'period': period,
        'status': status.toJson(),
        'preparedBy': preparedBy.toJson(),
        'approvalRequestedAt': approvalRequestedAt == null ? null : hrDateTimeToJson(approvalRequestedAt!),
        'approvedBy': approvedBy == null ? null : approvedBy!.toJson(),
        'approvedAt': approvedAt == null ? null : hrDateTimeToJson(approvedAt!),
        'closedAt': closedAt == null ? null : hrDateTimeToJson(closedAt!),
        'lastCalculatedAt': hrDateTimeToJson(lastCalculatedAt),
        'isStale': isStale,
        'employeeCount': employeeCount,
        'totals': totals.map((e) => e.toJson()).toList(),
        'concurrencyStamp': concurrencyStamp,
      };
}

class PayrollRunSummaryPagedResultDto {
  const PayrollRunSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<PayrollRunSummaryDto> items;

  factory PayrollRunSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => PayrollRunSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => PayrollRunSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class PayrollRunDto {
  const PayrollRunDto({
    required this.id,
    required this.period,
    required this.status,
    required this.preparedBy,
    this.approvalRequestedAt,
    this.approvedBy,
    this.approvedAt,
    this.closedAt,
    required this.lastCalculatedAt,
    required this.isStale,
    required this.employeeCount,
    required this.totals,
    required this.items,
    required this.employeesWithoutProfile,
    required this.pendingSuggestionCount,
    required this.concurrencyStamp,
  });

  final String id;
  final String period;
  final PayrollRunStatus status;
  final UserRefDto preparedBy;
  /// وقت طلب الاعتماد. يعود null بعد أي إعادة حساب.
  final DateTime? approvalRequestedAt;
  final UserRefDto? approvedBy;
  final DateTime? approvedAt;
  final DateTime? closedAt;
  final DateTime lastCalculatedAt;
  /// true إذا تغيرت المدخلات بعد آخر حساب. يبقى false بعد الاعتماد.
  final bool isStale;
  final int employeeCount;
  final List<PayrollTotalDto> totals;
  final List<PayrollItemDto> items;
  /// موظفون لا يملكون بيانات مالية سارية في الفترة، ولا يدخلون في الكشف.
  final List<EmployeeRefDto> employeesWithoutProfile;
  /// الاقتراحات غير المراجعة للفترة. لا تدخل في الحساب.
  final int pendingSuggestionCount;
  final String concurrencyStamp;

  factory PayrollRunDto.fromJson(Map<String, dynamic> json) => PayrollRunDto(
        id: (json['id']) as String,
        period: (json['period']) as String,
        status: PayrollRunStatus.fromJson(json['status']),
        preparedBy: UserRefDto.fromJson((json['preparedBy']) as Map<String, dynamic>),
        approvalRequestedAt: json['approvalRequestedAt'] == null ? null : DateTime.parse((json['approvalRequestedAt']) as String),
        approvedBy: json['approvedBy'] == null ? null : UserRefDto.fromJson((json['approvedBy']) as Map<String, dynamic>),
        approvedAt: json['approvedAt'] == null ? null : DateTime.parse((json['approvedAt']) as String),
        closedAt: json['closedAt'] == null ? null : DateTime.parse((json['closedAt']) as String),
        lastCalculatedAt: DateTime.parse((json['lastCalculatedAt']) as String),
        isStale: (json['isStale']) as bool,
        employeeCount: ((json['employeeCount']) as num).toInt(),
        totals: ((json['totals']) as List).map((e) => PayrollTotalDto.fromJson((e) as Map<String, dynamic>)).toList(),
        items: ((json['items']) as List).map((e) => PayrollItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
        employeesWithoutProfile: ((json['employeesWithoutProfile']) as List).map((e) => EmployeeRefDto.fromJson((e) as Map<String, dynamic>)).toList(),
        pendingSuggestionCount: ((json['pendingSuggestionCount']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'period': period,
        'status': status.toJson(),
        'preparedBy': preparedBy.toJson(),
        'approvalRequestedAt': approvalRequestedAt == null ? null : hrDateTimeToJson(approvalRequestedAt!),
        'approvedBy': approvedBy == null ? null : approvedBy!.toJson(),
        'approvedAt': approvedAt == null ? null : hrDateTimeToJson(approvedAt!),
        'closedAt': closedAt == null ? null : hrDateTimeToJson(closedAt!),
        'lastCalculatedAt': hrDateTimeToJson(lastCalculatedAt),
        'isStale': isStale,
        'employeeCount': employeeCount,
        'totals': totals.map((e) => e.toJson()).toList(),
        'items': items.map((e) => e.toJson()).toList(),
        'employeesWithoutProfile': employeesWithoutProfile.map((e) => e.toJson()).toList(),
        'pendingSuggestionCount': pendingSuggestionCount,
        'concurrencyStamp': concurrencyStamp,
      };
}

class PayrollRunCreateInputDto {
  const PayrollRunCreateInputDto({
    required this.period,
  });

  final String period;

  factory PayrollRunCreateInputDto.fromJson(Map<String, dynamic> json) => PayrollRunCreateInputDto(
        period: (json['period']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'period': period,
      };
}

class PayrollRunActionInputDto {
  const PayrollRunActionInputDto({
    required this.concurrencyStamp,
    this.note,
  });

  /// قيمة آخر قراءة، لضمان تنفيذ الإجراء على النسخة المعروضة.
  final String concurrencyStamp;
  final String? note;

  factory PayrollRunActionInputDto.fromJson(Map<String, dynamic> json) => PayrollRunActionInputDto(
        concurrencyStamp: (json['concurrencyStamp']) as String,
        note: json['note'] == null ? null : (json['note']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'concurrencyStamp': concurrencyStamp,
        if (note != null) 'note': note!,
      };
}

class PayslipSummaryDto {
  const PayslipSummaryDto({
    required this.id,
    required this.period,
    required this.currency,
    required this.gross,
    required this.additions,
    required this.deductions,
    required this.net,
    required this.closedAt,
  });

  final String id;
  final String period;
  /// ISO 4217.
  final String currency;
  final double gross;
  final double additions;
  final double deductions;
  final double net;
  final DateTime closedAt;

  factory PayslipSummaryDto.fromJson(Map<String, dynamic> json) => PayslipSummaryDto(
        id: (json['id']) as String,
        period: (json['period']) as String,
        currency: (json['currency']) as String,
        gross: ((json['gross']) as num).toDouble(),
        additions: ((json['additions']) as num).toDouble(),
        deductions: ((json['deductions']) as num).toDouble(),
        net: ((json['net']) as num).toDouble(),
        closedAt: DateTime.parse((json['closedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'period': period,
        'currency': currency,
        'gross': gross,
        'additions': additions,
        'deductions': deductions,
        'net': net,
        'closedAt': hrDateTimeToJson(closedAt),
      };
}

class PayslipSummaryPagedResultDto {
  const PayslipSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<PayslipSummaryDto> items;

  factory PayslipSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => PayslipSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => PayslipSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class PayslipDto {
  const PayslipDto({
    required this.id,
    required this.employee,
    required this.period,
    required this.currency,
    required this.gross,
    required this.additions,
    required this.deductions,
    required this.net,
    required this.lines,
    required this.closedAt,
  });

  final String id;
  final EmployeeRefDto employee;
  final String period;
  /// ISO 4217.
  final String currency;
  final double gross;
  final double additions;
  final double deductions;
  final double net;
  final List<PayrollLineDto> lines;
  final DateTime closedAt;

  factory PayslipDto.fromJson(Map<String, dynamic> json) => PayslipDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        period: (json['period']) as String,
        currency: (json['currency']) as String,
        gross: ((json['gross']) as num).toDouble(),
        additions: ((json['additions']) as num).toDouble(),
        deductions: ((json['deductions']) as num).toDouble(),
        net: ((json['net']) as num).toDouble(),
        lines: ((json['lines']) as List).map((e) => PayrollLineDto.fromJson((e) as Map<String, dynamic>)).toList(),
        closedAt: DateTime.parse((json['closedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'period': period,
        'currency': currency,
        'gross': gross,
        'additions': additions,
        'deductions': deductions,
        'net': net,
        'lines': lines.map((e) => e.toJson()).toList(),
        'closedAt': hrDateTimeToJson(closedAt),
      };
}

enum ExpenseSource {
  general('general'),
  storePurchase('storePurchase'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ExpenseSource(this.value);
  final String value;

  static ExpenseSource fromJson(Object? json) {
    for (final v in ExpenseSource.values) {
      if (v.value == json) return v;
    }
    return ExpenseSource.$unknown;
  }

  String toJson() => value;
}

enum ExpenseStatus {
  active('active'),
  reversed('reversed'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const ExpenseStatus(this.value);
  final String value;

  static ExpenseStatus fromJson(Object? json) {
    for (final v in ExpenseStatus.values) {
      if (v.value == json) return v;
    }
    return ExpenseStatus.$unknown;
  }

  String toJson() => value;
}

class ExpenseCreateInputDto {
  const ExpenseCreateInputDto({
    required this.category,
    required this.amount,
    required this.currency,
    required this.date,
    this.description,
    required this.attachment,
    this.sourceRequestId,
  });

  final String category;
  final double amount;
  /// ISO 4217.
  final String currency;
  final DateTime date;
  final String? description;
  /// مرفق واحد.
  final MultipartFile attachment;
  /// طلب المحاسب المرتبط، إن وجد.
  final String? sourceRequestId;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'category': category,
        'amount': amount,
        'currency': currency,
        'date': hrDateToJson(date),
        if (description != null) 'description': description!,
        if (sourceRequestId != null) 'sourceRequestId': sourceRequestId!,
      };

  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,
  /// one part per file for arrays (x-server-requirements.multipart).
  FormData toFormData() {
    final form = FormData();
    form.fields.add(MapEntry('category', hrFormValue(category)));
    form.fields.add(MapEntry('amount', hrFormValue(amount)));
    form.fields.add(MapEntry('currency', hrFormValue(currency)));
    form.fields.add(MapEntry('date', hrFormValue(hrDateToJson(date))));
    if (description != null) form.fields.add(MapEntry('description', hrFormValue(description!)));
    form.files.add(MapEntry('attachment', attachment));
    if (sourceRequestId != null) form.fields.add(MapEntry('sourceRequestId', hrFormValue(sourceRequestId!)));
    return form;
  }
}

class ExpenseDto {
  const ExpenseDto({
    required this.id,
    required this.category,
    required this.amount,
    required this.currency,
    required this.date,
    this.description,
    required this.source,
    required this.status,
    this.attachment,
    this.stockMovementId,
    this.sourceRequestId,
    this.reversalOfExpenseId,
    this.createdBy,
    required this.creationTime,
  });

  final String id;
  final String category;
  /// موجب، وسالب فقط في الحركة العكسية.
  final double amount;
  /// ISO 4217.
  final String currency;
  final DateTime date;
  final String? description;
  final ExpenseSource source;
  final ExpenseStatus status;
  final AttachmentDto? attachment;
  /// حركة المخزن عند source = storePurchase.
  final String? stockMovementId;
  final String? sourceRequestId;
  final String? reversalOfExpenseId;
  final UserRefDto? createdBy;
  final DateTime creationTime;

  factory ExpenseDto.fromJson(Map<String, dynamic> json) => ExpenseDto(
        id: (json['id']) as String,
        category: (json['category']) as String,
        amount: ((json['amount']) as num).toDouble(),
        currency: (json['currency']) as String,
        date: hrDateFromJson((json['date']) as String),
        description: json['description'] == null ? null : (json['description']) as String,
        source: ExpenseSource.fromJson(json['source']),
        status: ExpenseStatus.fromJson(json['status']),
        attachment: json['attachment'] == null ? null : AttachmentDto.fromJson((json['attachment']) as Map<String, dynamic>),
        stockMovementId: json['stockMovementId'] == null ? null : (json['stockMovementId']) as String,
        sourceRequestId: json['sourceRequestId'] == null ? null : (json['sourceRequestId']) as String,
        reversalOfExpenseId: json['reversalOfExpenseId'] == null ? null : (json['reversalOfExpenseId']) as String,
        createdBy: json['createdBy'] == null ? null : UserRefDto.fromJson((json['createdBy']) as Map<String, dynamic>),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'category': category,
        'amount': amount,
        'currency': currency,
        'date': hrDateToJson(date),
        'description': description == null ? null : description!,
        'source': source.toJson(),
        'status': status.toJson(),
        'attachment': attachment == null ? null : attachment!.toJson(),
        'stockMovementId': stockMovementId == null ? null : stockMovementId!,
        'sourceRequestId': sourceRequestId == null ? null : sourceRequestId!,
        'reversalOfExpenseId': reversalOfExpenseId == null ? null : reversalOfExpenseId!,
        'createdBy': createdBy == null ? null : createdBy!.toJson(),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class ExpensePagedResultDto {
  const ExpensePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<ExpenseDto> items;

  factory ExpensePagedResultDto.fromJson(Map<String, dynamic> json) => ExpensePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ExpenseDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

enum AccountantRequestStatus {
  pendingAccountant('pendingAccountant'),
  completed('completed'),
  rejected('rejected'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AccountantRequestStatus(this.value);
  final String value;

  static AccountantRequestStatus fromJson(Object? json) {
    for (final v in AccountantRequestStatus.values) {
      if (v.value == json) return v;
    }
    return AccountantRequestStatus.$unknown;
  }

  String toJson() => value;
}

class AccountantRequestSummaryDto {
  const AccountantRequestSummaryDto({
    required this.id,
    required this.employee,
    required this.requestTypeCode,
    required this.requestTypeName,
    required this.status,
    required this.arrivedAt,
    this.decidedAt,
  });

  /// معرف الطلب نفسه في وحدة الطلبات.
  final String id;
  final EmployeeRefDto employee;
  final String requestTypeCode;
  /// Localized by Accept-Language.
  final String requestTypeName;
  final AccountantRequestStatus status;
  /// وقت وصول الطلب إلى خطوة المحاسب.
  final DateTime arrivedAt;
  final DateTime? decidedAt;

  factory AccountantRequestSummaryDto.fromJson(Map<String, dynamic> json) => AccountantRequestSummaryDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        requestTypeCode: (json['requestTypeCode']) as String,
        requestTypeName: (json['requestTypeName']) as String,
        status: AccountantRequestStatus.fromJson(json['status']),
        arrivedAt: DateTime.parse((json['arrivedAt']) as String),
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse((json['decidedAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'requestTypeCode': requestTypeCode,
        'requestTypeName': requestTypeName,
        'status': status.toJson(),
        'arrivedAt': hrDateTimeToJson(arrivedAt),
        'decidedAt': decidedAt == null ? null : hrDateTimeToJson(decidedAt!),
      };
}

class AccountantRequestSummaryPagedResultDto {
  const AccountantRequestSummaryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<AccountantRequestSummaryDto> items;

  factory AccountantRequestSummaryPagedResultDto.fromJson(Map<String, dynamic> json) => AccountantRequestSummaryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AccountantRequestSummaryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class AccountantRequestDto {
  const AccountantRequestDto({
    required this.id,
    required this.employee,
    required this.submittedBy,
    required this.requestTypeCode,
    required this.requestTypeName,
    this.description,
    required this.status,
    required this.arrivedAt,
    this.decidedAt,
    required this.timeline,
    required this.linkedAdjustmentIds,
    required this.linkedExpenseIds,
  });

  /// معرف الطلب نفسه في وحدة الطلبات.
  final String id;
  final EmployeeRefDto employee;
  final UserRefDto submittedBy;
  final String requestTypeCode;
  /// Localized by Accept-Language.
  final String requestTypeName;
  final String? description;
  final AccountantRequestStatus status;
  /// وقت وصول الطلب إلى خطوة المحاسب.
  final DateTime arrivedAt;
  final DateTime? decidedAt;
  final List<TimelineEntryDto> timeline;
  final List<String> linkedAdjustmentIds;
  final List<String> linkedExpenseIds;

  factory AccountantRequestDto.fromJson(Map<String, dynamic> json) => AccountantRequestDto(
        id: (json['id']) as String,
        employee: EmployeeRefDto.fromJson((json['employee']) as Map<String, dynamic>),
        submittedBy: UserRefDto.fromJson((json['submittedBy']) as Map<String, dynamic>),
        requestTypeCode: (json['requestTypeCode']) as String,
        requestTypeName: (json['requestTypeName']) as String,
        description: json['description'] == null ? null : (json['description']) as String,
        status: AccountantRequestStatus.fromJson(json['status']),
        arrivedAt: DateTime.parse((json['arrivedAt']) as String),
        decidedAt: json['decidedAt'] == null ? null : DateTime.parse((json['decidedAt']) as String),
        timeline: ((json['timeline']) as List).map((e) => TimelineEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
        linkedAdjustmentIds: ((json['linkedAdjustmentIds']) as List).map((e) => (e) as String).toList(),
        linkedExpenseIds: ((json['linkedExpenseIds']) as List).map((e) => (e) as String).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'employee': employee.toJson(),
        'submittedBy': submittedBy.toJson(),
        'requestTypeCode': requestTypeCode,
        'requestTypeName': requestTypeName,
        'description': description == null ? null : description!,
        'status': status.toJson(),
        'arrivedAt': hrDateTimeToJson(arrivedAt),
        'decidedAt': decidedAt == null ? null : hrDateTimeToJson(decidedAt!),
        'timeline': timeline.map((e) => e.toJson()).toList(),
        'linkedAdjustmentIds': linkedAdjustmentIds,
        'linkedExpenseIds': linkedExpenseIds,
      };
}

enum FinanceExportFormat {
  pdf('pdf'),
  excel('excel'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const FinanceExportFormat(this.value);
  final String value;

  static FinanceExportFormat fromJson(Object? json) {
    for (final v in FinanceExportFormat.values) {
      if (v.value == json) return v;
    }
    return FinanceExportFormat.$unknown;
  }

  String toJson() => value;
}

enum FinanceReportCode {
  monthlyStatement('monthlyStatement'),
  adjustments('adjustments'),
  storeExpenses('storeExpenses'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const FinanceReportCode(this.value);
  final String value;

  static FinanceReportCode fromJson(Object? json) {
    for (final v in FinanceReportCode.values) {
      if (v.value == json) return v;
    }
    return FinanceReportCode.$unknown;
  }

  String toJson() => value;
}

class FinanceMonthlyStatementReportDto {
  const FinanceMonthlyStatementReportDto({
    required this.period,
    required this.runId,
    required this.runStatus,
    required this.items,
    required this.totals,
  });

  final String period;
  final String runId;
  final PayrollRunStatus runStatus;
  final List<PayrollItemDto> items;
  final List<PayrollTotalDto> totals;

  factory FinanceMonthlyStatementReportDto.fromJson(Map<String, dynamic> json) => FinanceMonthlyStatementReportDto(
        period: (json['period']) as String,
        runId: (json['runId']) as String,
        runStatus: PayrollRunStatus.fromJson(json['runStatus']),
        items: ((json['items']) as List).map((e) => PayrollItemDto.fromJson((e) as Map<String, dynamic>)).toList(),
        totals: ((json['totals']) as List).map((e) => PayrollTotalDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'period': period,
        'runId': runId,
        'runStatus': runStatus.toJson(),
        'items': items.map((e) => e.toJson()).toList(),
        'totals': totals.map((e) => e.toJson()).toList(),
      };
}

class FinanceAdjustmentTotalDto {
  const FinanceAdjustmentTotalDto({
    required this.currency,
    required this.additions,
    required this.deductions,
  });

  /// ISO 4217.
  final String currency;
  final double additions;
  final double deductions;

  factory FinanceAdjustmentTotalDto.fromJson(Map<String, dynamic> json) => FinanceAdjustmentTotalDto(
        currency: (json['currency']) as String,
        additions: ((json['additions']) as num).toDouble(),
        deductions: ((json['deductions']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'currency': currency,
        'additions': additions,
        'deductions': deductions,
      };
}

class FinanceAdjustmentReportPagedResultDto {
  const FinanceAdjustmentReportPagedResultDto({
    required this.totalCount,
    required this.items,
    required this.totals,
  });

  final int totalCount;
  final List<AdjustmentDto> items;
  final List<FinanceAdjustmentTotalDto> totals;

  factory FinanceAdjustmentReportPagedResultDto.fromJson(Map<String, dynamic> json) => FinanceAdjustmentReportPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AdjustmentDto.fromJson((e) as Map<String, dynamic>)).toList(),
        totals: ((json['totals']) as List).map((e) => FinanceAdjustmentTotalDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
        'totals': totals.map((e) => e.toJson()).toList(),
      };
}

class FinanceAmountTotalDto {
  const FinanceAmountTotalDto({
    required this.currency,
    required this.amount,
  });

  /// ISO 4217.
  final String currency;
  final double amount;

  factory FinanceAmountTotalDto.fromJson(Map<String, dynamic> json) => FinanceAmountTotalDto(
        currency: (json['currency']) as String,
        amount: ((json['amount']) as num).toDouble(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'currency': currency,
        'amount': amount,
      };
}

class FinanceStoreExpenseReportPagedResultDto {
  const FinanceStoreExpenseReportPagedResultDto({
    required this.totalCount,
    required this.items,
    required this.totals,
  });

  final int totalCount;
  final List<ExpenseDto> items;
  final List<FinanceAmountTotalDto> totals;

  factory FinanceStoreExpenseReportPagedResultDto.fromJson(Map<String, dynamic> json) => FinanceStoreExpenseReportPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => ExpenseDto.fromJson((e) as Map<String, dynamic>)).toList(),
        totals: ((json['totals']) as List).map((e) => FinanceAmountTotalDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
        'totals': totals.map((e) => e.toJson()).toList(),
      };
}

/// رمز القالب الذي يحدد وجهة التطبيق. عند ظهور رمز غير معروف يفتح التطبيق قائمة الإشعارات. | templateCode | resourceType | المستلم | وجهة التطبيق | |---|---|---|---| | requestPendingAction | request | المعتمد الحالي، ومنه المحاسب في خطوته | تفاصيل الطلب: GET /api/app/request/{resourceId} | | requestStatusChanged | request | صاحب الطلب | تفاصيل الطلب: GET /api/app/request/{resourceId} | | requestSubmi...
enum NotificationTemplateCode {
  requestPendingAction('requestPendingAction'),
  requestStatusChanged('requestStatusChanged'),
  requestSubmittedOnBehalf('requestSubmittedOnBehalf'),
  requestApprovalReminder('requestApprovalReminder'),
  memoUpdate('memoUpdate'),
  incidentUpdate('incidentUpdate'),
  stockIssuePending('stockIssuePending'),
  custodyAssigned('custodyAssigned'),
  circularReceived('circularReceived'),
  monthEndCycleOpened('monthEndCycleOpened'),
  monthlyCycleReminder('monthlyCycleReminder'),
  monthlyCycleEscalated('monthlyCycleEscalated'),
  evaluationPublished('evaluationPublished'),
  evaluationResponseReceived('evaluationResponseReceived'),
  evaluationObjectionDecided('evaluationObjectionDecided'),
  contractExpiring('contractExpiring'),
  contractDecisionDue('contractDecisionDue'),
  contractDecided('contractDecided'),
  warningRecommended('warningRecommended'),
  warningNotified('warningNotified'),
  warningReadyForDecision('warningReadyForDecision'),
  warningDecided('warningDecided'),
  warningObjectionSubmitted('warningObjectionSubmitted'),
  warningObjectionResolved('warningObjectionResolved'),
  warningObjectionResolvedForHr('warningObjectionResolvedForHr'),
  appreciationLetterIssued('appreciationLetterIssued'),
  payrollRunPendingApproval('payrollRunPendingApproval'),
  payrollRunApproved('payrollRunApproved'),
  payslipAvailable('payslipAvailable'),
  adjustmentSuggestionsReady('adjustmentSuggestionsReady'),
  accountPasswordReset('accountPasswordReset'),
  secureItemUpdateForOwner('secureItemUpdateForOwner'),
  secureItemUpdateForHandler('secureItemUpdateForHandler'),
  secureHandlerAssignmentUpdate('secureHandlerAssignmentUpdate'),
  delegationUpdated('delegationUpdated'),
  managerChanged('managerChanged'),
  leaveBalanceAdjusted('leaveBalanceAdjusted'),
  accountSecurityChanged('accountSecurityChanged'),
  custodyReturned('custodyReturned'),
  custodyCountDifference('custodyCountDifference'),
  reviewReportSubmitted('reviewReportSubmitted'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const NotificationTemplateCode(this.value);
  final String value;

  static NotificationTemplateCode fromJson(Object? json) {
    for (final v in NotificationTemplateCode.values) {
      if (v.value == json) return v;
    }
    return NotificationTemplateCode.$unknown;
  }

  String toJson() => value;
}

/// نوع المورد الذي يشير إليه resourceId. القيمة none تعني عدم وجود مورد.
enum NotificationResourceType {
  request('request'),
  memo('memo'),
  incident('incident'),
  stockIssue('stockIssue'),
  custody('custody'),
  circular('circular'),
  evaluation('evaluation'),
  contract('contract'),
  warning('warning'),
  appreciationLetter('appreciationLetter'),
  payrollRun('payrollRun'),
  payslip('payslip'),
  adjustmentSuggestion('adjustmentSuggestion'),
  none('none'),
  secureItem('secureItem'),
  reviewReport('reviewReport'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const NotificationResourceType(this.value);
  final String value;

  static NotificationResourceType fromJson(Object? json) {
    for (final v in NotificationResourceType.values) {
      if (v.value == json) return v;
    }
    return NotificationResourceType.$unknown;
  }

  String toJson() => value;
}

/// الإشعار داخل النظام، وهو أيضًا حمولة حدث SignalR. عند secureItemUpdateForOwner يكون النص دائمًا «لديك تحديث جديد» بما يقابله بالإنجليزية.
class NotificationDto {
  const NotificationDto({
    required this.id,
    required this.templateCode,
    required this.resourceType,
    this.resourceId,
    this.count,
    required this.title,
    required this.body,
    required this.isRead,
    this.readAt,
    required this.creationTime,
  });

  final String id;
  final NotificationTemplateCode templateCode;
  final NotificationResourceType resourceType;
  final String? resourceId;
  /// عدد للعرض في التنبيهات المجمعة فقط. يبقى null في إشعارات الشكاوى.
  final int? count;
  /// نص من القالب بحسب Accept-Language.
  final String title;
  /// نص من القالب بحسب Accept-Language، دون اسم أو موضوع أو مستوى شكوى، ودون مبالغ.
  final String body;
  final bool isRead;
  final DateTime? readAt;
  final DateTime creationTime;

  factory NotificationDto.fromJson(Map<String, dynamic> json) => NotificationDto(
        id: (json['id']) as String,
        templateCode: NotificationTemplateCode.fromJson(json['templateCode']),
        resourceType: NotificationResourceType.fromJson(json['resourceType']),
        resourceId: json['resourceId'] == null ? null : (json['resourceId']) as String,
        count: json['count'] == null ? null : ((json['count']) as num).toInt(),
        title: (json['title']) as String,
        body: (json['body']) as String,
        isRead: (json['isRead']) as bool,
        readAt: json['readAt'] == null ? null : DateTime.parse((json['readAt']) as String),
        creationTime: DateTime.parse((json['creationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'templateCode': templateCode.toJson(),
        'resourceType': resourceType.toJson(),
        'resourceId': resourceId == null ? null : resourceId!,
        'count': count == null ? null : count!,
        'title': title,
        'body': body,
        'isRead': isRead,
        'readAt': readAt == null ? null : hrDateTimeToJson(readAt!),
        'creationTime': hrDateTimeToJson(creationTime),
      };
}

class NotificationPagedResultDto {
  const NotificationPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<NotificationDto> items;

  factory NotificationPagedResultDto.fromJson(Map<String, dynamic> json) => NotificationPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => NotificationDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class NotificationUnreadCountDto {
  const NotificationUnreadCountDto({
    required this.count,
  });

  final int count;

  factory NotificationUnreadCountDto.fromJson(Map<String, dynamic> json) => NotificationUnreadCountDto(
        count: ((json['count']) as num).toInt(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'count': count,
      };
}

/// حقول data في رسائل FCM وAPNs. لا تضاف إليها أي حقول أخرى. لا تحمل اسمًا أو موضوعًا أو مستوى شكوى، ولا مبالغ. يوجه التطبيق المستخدم عبر templateCode وresourceId، ثم يجلب التفاصيل من الـAPI بصلاحية المستخدم. نص العرض في الهاتف والبريد يكون بلغة المستلم المحفوظة في NotificationPreferenceDto.
class NotificationPushPayloadDto {
  const NotificationPushPayloadDto({
    required this.notificationId,
    required this.templateCode,
    required this.resourceType,
    this.resourceId,
    this.count,
  });

  final String notificationId;
  final NotificationTemplateCode templateCode;
  final NotificationResourceType resourceType;
  /// يُحذف إذا لم يوجد مورد.
  final String? resourceId;
  /// العدد كنص، لأن حقول data في FCM نصية. لا يرسل في إشعارات الشكاوى.
  final String? count;

  factory NotificationPushPayloadDto.fromJson(Map<String, dynamic> json) => NotificationPushPayloadDto(
        notificationId: (json['notificationId']) as String,
        templateCode: NotificationTemplateCode.fromJson(json['templateCode']),
        resourceType: NotificationResourceType.fromJson(json['resourceType']),
        resourceId: json['resourceId'] == null ? null : (json['resourceId']) as String,
        count: json['count'] == null ? null : (json['count']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'notificationId': notificationId,
        'templateCode': templateCode.toJson(),
        'resourceType': resourceType.toJson(),
        if (resourceId != null) 'resourceId': resourceId!,
        if (count != null) 'count': count!,
      };
}

enum DevicePlatform {
  android('android'),
  ios('ios'),
  web('web'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const DevicePlatform(this.value);
  final String value;

  static DevicePlatform fromJson(Object? json) {
    for (final v in DevicePlatform.values) {
      if (v.value == json) return v;
    }
    return DevicePlatform.$unknown;
  }

  String toJson() => value;
}

class DeviceTokenRegisterInputDto {
  const DeviceTokenRegisterInputDto({
    required this.token,
    required this.platform,
  });

  final String token;
  final DevicePlatform platform;

  factory DeviceTokenRegisterInputDto.fromJson(Map<String, dynamic> json) => DeviceTokenRegisterInputDto(
        token: (json['token']) as String,
        platform: DevicePlatform.fromJson(json['platform']),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'token': token,
        'platform': platform.toJson(),
      };
}

class DeviceTokenUnregisterInputDto {
  const DeviceTokenUnregisterInputDto({
    required this.token,
  });

  final String token;

  factory DeviceTokenUnregisterInputDto.fromJson(Map<String, dynamic> json) => DeviceTokenUnregisterInputDto(
        token: (json['token']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'token': token,
      };
}

/// أقسام الإعدادات ومن يديرها وفق 5.18. | القسم | يديره | مكان الإدارة | |---|---|---| | reminders | systemAdmin | /api/app/system-settings/reminders | | contracts | systemAdmin | /api/app/system-settings/contracts | | complaints | systemAdmin | /api/app/system-settings/complaints | | attendance | systemAdmin | /api/app/system-settings/attendance | | hourlyLeave | systemAdmin | /api/app/system-settin...
enum SystemSettingsSectionCode {
  reminders('reminders'),
  contracts('contracts'),
  complaints('complaints'),
  attendance('attendance'),
  hourlyLeave('hourlyLeave'),
  finance('finance'),
  rolesAndPermissions('rolesAndPermissions'),
  requestTypes('requestTypes'),
  otherLeaveTypes('otherLeaveTypes'),
  attendanceDevices('attendanceDevices'),
  evaluationCriteria('evaluationCriteria'),
  employeeLeaveBalances('employeeLeaveBalances'),
  workShifts('workShifts'),
  holidays('holidays'),
  departmentsAndManagerLinks('departmentsAndManagerLinks'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const SystemSettingsSectionCode(this.value);
  final String value;

  static SystemSettingsSectionCode fromJson(Object? json) {
    for (final v in SystemSettingsSectionCode.values) {
      if (v.value == json) return v;
    }
    return SystemSettingsSectionCode.$unknown;
  }

  String toJson() => value;
}

class SystemSettingsSectionDto {
  const SystemSettingsSectionDto({
    required this.code,
    required this.ownerRole,
    required this.isEditableByCaller,
  });

  final SystemSettingsSectionCode code;
  final AppRole ownerRole;
  final bool isEditableByCaller;

  factory SystemSettingsSectionDto.fromJson(Map<String, dynamic> json) => SystemSettingsSectionDto(
        code: SystemSettingsSectionCode.fromJson(json['code']),
        ownerRole: AppRole.fromJson(json['ownerRole']),
        isEditableByCaller: (json['isEditableByCaller']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'code': code.toJson(),
        'ownerRole': ownerRole.toJson(),
        'isEditableByCaller': isEditableByCaller,
      };
}

class SystemSettingsSectionListDto {
  const SystemSettingsSectionListDto({
    required this.items,
  });

  final List<SystemSettingsSectionDto> items;

  factory SystemSettingsSectionListDto.fromJson(Map<String, dynamic> json) => SystemSettingsSectionListDto(
        items: ((json['items']) as List).map((e) => SystemSettingsSectionDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'items': items.map((e) => e.toJson()).toList(),
      };
}

/// الحقول null لا قيمة لها حتى يحددها مسؤول النظام، ولا يعمل ما يعتمد عليها قبل ذلك.
class SystemSettingsRemindersDto {
  const SystemSettingsRemindersDto({
    required this.monthEndOpenTime,
    required this.monthlySubmissionWorkingDays,
    required this.monthlyReminderIntervalWorkingDays,
    required this.monthlyEscalationWorkingDay,
    this.approvalReminderIntervalWorkingDays,
    this.evaluationMinWorkedDays,
    required this.concurrencyStamp,
    this.lastModificationTime,
  });

  /// وقت فتح النماذج في آخر يوم دوام. القيمة الأولية 10:00.
  final String monthEndOpenTime;
  /// أيام دوام مهلة رفع التقييم والتقرير من بداية الشهر التالي. القيمة الأولية 3.
  final int monthlySubmissionWorkingDays;
  /// الفاصل بين تذكيرات المهلة بأيام الدوام. القيمة الأولية 1.
  final int monthlyReminderIntervalWorkingDays;
  /// يوم الدوام الذي يتم فيه التصعيد لمدير الموارد البشرية وإظهار «متأخر» للمراجع. يجب أن يكون بعد المهلة. القيمة الأولية 4.
  final int monthlyEscalationWorkingDay;
  /// فاصل التذكير الدوري بالموافقات المعلقة، دون إجراء تلقائي.
  final int? approvalReminderIntervalWorkingDays;
  /// أقل مدة عمل في الشهر لتقييم من باشر في منتصفه.
  final int? evaluationMinWorkedDays;
  final String concurrencyStamp;
  final DateTime? lastModificationTime;

  factory SystemSettingsRemindersDto.fromJson(Map<String, dynamic> json) => SystemSettingsRemindersDto(
        monthEndOpenTime: (json['monthEndOpenTime']) as String,
        monthlySubmissionWorkingDays: ((json['monthlySubmissionWorkingDays']) as num).toInt(),
        monthlyReminderIntervalWorkingDays: ((json['monthlyReminderIntervalWorkingDays']) as num).toInt(),
        monthlyEscalationWorkingDay: ((json['monthlyEscalationWorkingDay']) as num).toInt(),
        approvalReminderIntervalWorkingDays: json['approvalReminderIntervalWorkingDays'] == null ? null : ((json['approvalReminderIntervalWorkingDays']) as num).toInt(),
        evaluationMinWorkedDays: json['evaluationMinWorkedDays'] == null ? null : ((json['evaluationMinWorkedDays']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'monthEndOpenTime': monthEndOpenTime,
        'monthlySubmissionWorkingDays': monthlySubmissionWorkingDays,
        'monthlyReminderIntervalWorkingDays': monthlyReminderIntervalWorkingDays,
        'monthlyEscalationWorkingDay': monthlyEscalationWorkingDay,
        'approvalReminderIntervalWorkingDays': approvalReminderIntervalWorkingDays == null ? null : approvalReminderIntervalWorkingDays!,
        'evaluationMinWorkedDays': evaluationMinWorkedDays == null ? null : evaluationMinWorkedDays!,
        'concurrencyStamp': concurrencyStamp,
        'lastModificationTime': lastModificationTime == null ? null : hrDateTimeToJson(lastModificationTime!),
      };
}

class SystemSettingsRemindersUpdateInputDto {
  const SystemSettingsRemindersUpdateInputDto({
    required this.monthEndOpenTime,
    required this.monthlySubmissionWorkingDays,
    required this.monthlyReminderIntervalWorkingDays,
    required this.monthlyEscalationWorkingDay,
    required this.approvalReminderIntervalWorkingDays,
    required this.evaluationMinWorkedDays,
    required this.concurrencyStamp,
  });

  /// وقت فتح النماذج في آخر يوم دوام. القيمة الأولية 10:00.
  final String monthEndOpenTime;
  /// أيام دوام مهلة رفع التقييم والتقرير من بداية الشهر التالي. القيمة الأولية 3.
  final int monthlySubmissionWorkingDays;
  /// الفاصل بين تذكيرات المهلة بأيام الدوام. القيمة الأولية 1.
  final int monthlyReminderIntervalWorkingDays;
  /// يوم الدوام الذي يتم فيه التصعيد لمدير الموارد البشرية وإظهار «متأخر» للمراجع. يجب أن يكون بعد المهلة. القيمة الأولية 4.
  final int monthlyEscalationWorkingDay;
  /// فاصل التذكير الدوري بالموافقات المعلقة، دون إجراء تلقائي.
  final int approvalReminderIntervalWorkingDays;
  /// أقل مدة عمل في الشهر لتقييم من باشر في منتصفه.
  final int evaluationMinWorkedDays;
  final String concurrencyStamp;

  factory SystemSettingsRemindersUpdateInputDto.fromJson(Map<String, dynamic> json) => SystemSettingsRemindersUpdateInputDto(
        monthEndOpenTime: (json['monthEndOpenTime']) as String,
        monthlySubmissionWorkingDays: ((json['monthlySubmissionWorkingDays']) as num).toInt(),
        monthlyReminderIntervalWorkingDays: ((json['monthlyReminderIntervalWorkingDays']) as num).toInt(),
        monthlyEscalationWorkingDay: ((json['monthlyEscalationWorkingDay']) as num).toInt(),
        approvalReminderIntervalWorkingDays: ((json['approvalReminderIntervalWorkingDays']) as num).toInt(),
        evaluationMinWorkedDays: ((json['evaluationMinWorkedDays']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'monthEndOpenTime': monthEndOpenTime,
        'monthlySubmissionWorkingDays': monthlySubmissionWorkingDays,
        'monthlyReminderIntervalWorkingDays': monthlyReminderIntervalWorkingDays,
        'monthlyEscalationWorkingDay': monthlyEscalationWorkingDay,
        'approvalReminderIntervalWorkingDays': approvalReminderIntervalWorkingDays,
        'evaluationMinWorkedDays': evaluationMinWorkedDays,
        'concurrencyStamp': concurrencyStamp,
      };
}

class SystemSettingsContractsDto {
  const SystemSettingsContractsDto({
    required this.alertDays,
    required this.concurrencyStamp,
    this.lastModificationTime,
  });

  /// الأيام التقويمية قبل انتهاء العقد. القيمة الأولية 30.
  final int alertDays;
  final String concurrencyStamp;
  final DateTime? lastModificationTime;

  factory SystemSettingsContractsDto.fromJson(Map<String, dynamic> json) => SystemSettingsContractsDto(
        alertDays: ((json['alertDays']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'alertDays': alertDays,
        'concurrencyStamp': concurrencyStamp,
        'lastModificationTime': lastModificationTime == null ? null : hrDateTimeToJson(lastModificationTime!),
      };
}

class SystemSettingsContractsUpdateInputDto {
  const SystemSettingsContractsUpdateInputDto({
    required this.alertDays,
    required this.concurrencyStamp,
  });

  final int alertDays;
  final String concurrencyStamp;

  factory SystemSettingsContractsUpdateInputDto.fromJson(Map<String, dynamic> json) => SystemSettingsContractsUpdateInputDto(
        alertDays: ((json['alertDays']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'alertDays': alertDays,
        'concurrencyStamp': concurrencyStamp,
      };
}

/// مهل فقط، دون أي محتوى أو أعداد أو حقوق معالجة.
class SystemSettingsComplaintsDto {
  const SystemSettingsComplaintsDto({
    this.delayDeadlineWorkingDays,
    this.overdueReminderIntervalWorkingDays,
    required this.concurrencyStamp,
    this.lastModificationTime,
  });

  /// المدة التي تعد بعدها الشكوى متأخرة.
  final int? delayDeadlineWorkingDays;
  /// الفاصل بين تذكيرات الشكاوى المتأخرة.
  final int? overdueReminderIntervalWorkingDays;
  final String concurrencyStamp;
  final DateTime? lastModificationTime;

  factory SystemSettingsComplaintsDto.fromJson(Map<String, dynamic> json) => SystemSettingsComplaintsDto(
        delayDeadlineWorkingDays: json['delayDeadlineWorkingDays'] == null ? null : ((json['delayDeadlineWorkingDays']) as num).toInt(),
        overdueReminderIntervalWorkingDays: json['overdueReminderIntervalWorkingDays'] == null ? null : ((json['overdueReminderIntervalWorkingDays']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'delayDeadlineWorkingDays': delayDeadlineWorkingDays == null ? null : delayDeadlineWorkingDays!,
        'overdueReminderIntervalWorkingDays': overdueReminderIntervalWorkingDays == null ? null : overdueReminderIntervalWorkingDays!,
        'concurrencyStamp': concurrencyStamp,
        'lastModificationTime': lastModificationTime == null ? null : hrDateTimeToJson(lastModificationTime!),
      };
}

class SystemSettingsComplaintsUpdateInputDto {
  const SystemSettingsComplaintsUpdateInputDto({
    required this.delayDeadlineWorkingDays,
    required this.overdueReminderIntervalWorkingDays,
    required this.concurrencyStamp,
  });

  /// المدة التي تعد بعدها الشكوى متأخرة.
  final int delayDeadlineWorkingDays;
  /// الفاصل بين تذكيرات الشكاوى المتأخرة.
  final int overdueReminderIntervalWorkingDays;
  final String concurrencyStamp;

  factory SystemSettingsComplaintsUpdateInputDto.fromJson(Map<String, dynamic> json) => SystemSettingsComplaintsUpdateInputDto(
        delayDeadlineWorkingDays: ((json['delayDeadlineWorkingDays']) as num).toInt(),
        overdueReminderIntervalWorkingDays: ((json['overdueReminderIntervalWorkingDays']) as num).toInt(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'delayDeadlineWorkingDays': delayDeadlineWorkingDays,
        'overdueReminderIntervalWorkingDays': overdueReminderIntervalWorkingDays,
        'concurrencyStamp': concurrencyStamp,
      };
}

class SystemSettingsHourlyLeaveDto {
  const SystemSettingsHourlyLeaveDto({
    this.monthlyAllowedHours,
    this.hoursPerDay,
    required this.concurrencyStamp,
    this.lastModificationTime,
  });

  /// الساعات الزمنية المسموحة في الشهر.
  final double? monthlyAllowedHours;
  /// عدد الساعات التي تساوي يومًا عند الخصم من الرصيد السنوي.
  final double? hoursPerDay;
  final String concurrencyStamp;
  final DateTime? lastModificationTime;

  factory SystemSettingsHourlyLeaveDto.fromJson(Map<String, dynamic> json) => SystemSettingsHourlyLeaveDto(
        monthlyAllowedHours: json['monthlyAllowedHours'] == null ? null : ((json['monthlyAllowedHours']) as num).toDouble(),
        hoursPerDay: json['hoursPerDay'] == null ? null : ((json['hoursPerDay']) as num).toDouble(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'monthlyAllowedHours': monthlyAllowedHours == null ? null : monthlyAllowedHours!,
        'hoursPerDay': hoursPerDay == null ? null : hoursPerDay!,
        'concurrencyStamp': concurrencyStamp,
        'lastModificationTime': lastModificationTime == null ? null : hrDateTimeToJson(lastModificationTime!),
      };
}

class SystemSettingsHourlyLeaveUpdateInputDto {
  const SystemSettingsHourlyLeaveUpdateInputDto({
    required this.monthlyAllowedHours,
    required this.hoursPerDay,
    required this.concurrencyStamp,
  });

  /// الساعات الزمنية المسموحة في الشهر.
  final double monthlyAllowedHours;
  /// عدد الساعات التي تساوي يومًا عند الخصم من الرصيد السنوي.
  final double hoursPerDay;
  final String concurrencyStamp;

  factory SystemSettingsHourlyLeaveUpdateInputDto.fromJson(Map<String, dynamic> json) => SystemSettingsHourlyLeaveUpdateInputDto(
        monthlyAllowedHours: ((json['monthlyAllowedHours']) as num).toDouble(),
        hoursPerDay: ((json['hoursPerDay']) as num).toDouble(),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'monthlyAllowedHours': monthlyAllowedHours,
        'hoursPerDay': hoursPerDay,
        'concurrencyStamp': concurrencyStamp,
      };
}

enum PayrollApproverRole {
  hrManager('hrManager'),
  executiveDirector('executiveDirector'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const PayrollApproverRole(this.value);
  final String value;

  static PayrollApproverRole fromJson(Object? json) {
    for (final v in PayrollApproverRole.values) {
      if (v.value == json) return v;
    }
    return PayrollApproverRole.$unknown;
  }

  String toJson() => value;
}

/// لا يتضمن أي بيانات مالية.
class SystemSettingsFinanceDto {
  const SystemSettingsFinanceDto({
    this.payrollApproverRole,
    required this.concurrencyStamp,
    this.lastModificationTime,
  });

  final PayrollApproverRole? payrollApproverRole;
  final String concurrencyStamp;
  final DateTime? lastModificationTime;

  factory SystemSettingsFinanceDto.fromJson(Map<String, dynamic> json) => SystemSettingsFinanceDto(
        payrollApproverRole: json['payrollApproverRole'] == null ? null : PayrollApproverRole.fromJson(json['payrollApproverRole']),
        concurrencyStamp: (json['concurrencyStamp']) as String,
        lastModificationTime: json['lastModificationTime'] == null ? null : DateTime.parse((json['lastModificationTime']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'payrollApproverRole': payrollApproverRole == null ? null : payrollApproverRole!.toJson(),
        'concurrencyStamp': concurrencyStamp,
        'lastModificationTime': lastModificationTime == null ? null : hrDateTimeToJson(lastModificationTime!),
      };
}

class SystemSettingsFinanceUpdateInputDto {
  const SystemSettingsFinanceUpdateInputDto({
    required this.payrollApproverRole,
    required this.concurrencyStamp,
  });

  final PayrollApproverRole payrollApproverRole;
  final String concurrencyStamp;

  factory SystemSettingsFinanceUpdateInputDto.fromJson(Map<String, dynamic> json) => SystemSettingsFinanceUpdateInputDto(
        payrollApproverRole: PayrollApproverRole.fromJson(json['payrollApproverRole']),
        concurrencyStamp: (json['concurrencyStamp']) as String,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'payrollApproverRole': payrollApproverRole.toJson(),
        'concurrencyStamp': concurrencyStamp,
      };
}

enum AuditActionType {
  create('create'),
  update('update'),
  delete('delete'),
  read('read'),
  export('export'),
  login('login'),
  loginFailed('loginFailed'),
  passwordReset('passwordReset'),
  roleChanged('roleChanged'),
  permissionChanged('permissionChanged'),
  settingChanged('settingChanged'),
  complaintHandlerAssignment('complaintHandlerAssignment'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AuditActionType(this.value);
  final String value;

  static AuditActionType fromJson(Object? json) {
    for (final v in AuditActionType.values) {
      if (v.value == json) return v;
    }
    return AuditActionType.$unknown;
  }

  String toJson() => value;
}

/// لا يتضمن قيم البيانات أو معاملات الطلبات. لا يعرض كيانات وحدة الشكاوى ولا ComplaintAccessLogs.
class AuditLogEntryDto {
  const AuditLogEntryDto({
    required this.id,
    required this.occurredAt,
    this.user,
    required this.action,
    this.entityType,
    this.entityId,
    required this.changedFields,
    this.ipAddress,
    required this.succeeded,
  });

  final String id;
  final DateTime occurredAt;
  final UserRefDto? user;
  final AuditActionType action;
  final String? entityType;
  final String? entityId;
  /// أسماء الحقول المتغيرة فقط، دون القيم.
  final List<String> changedFields;
  final String? ipAddress;
  final bool succeeded;

  factory AuditLogEntryDto.fromJson(Map<String, dynamic> json) => AuditLogEntryDto(
        id: (json['id']) as String,
        occurredAt: DateTime.parse((json['occurredAt']) as String),
        user: json['user'] == null ? null : UserRefDto.fromJson((json['user']) as Map<String, dynamic>),
        action: AuditActionType.fromJson(json['action']),
        entityType: json['entityType'] == null ? null : (json['entityType']) as String,
        entityId: json['entityId'] == null ? null : (json['entityId']) as String,
        changedFields: ((json['changedFields']) as List).map((e) => (e) as String).toList(),
        ipAddress: json['ipAddress'] == null ? null : (json['ipAddress']) as String,
        succeeded: (json['succeeded']) as bool,
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'id': id,
        'occurredAt': hrDateTimeToJson(occurredAt),
        'user': user == null ? null : user!.toJson(),
        'action': action.toJson(),
        'entityType': entityType == null ? null : entityType!,
        'entityId': entityId == null ? null : entityId!,
        'changedFields': changedFields,
        'ipAddress': ipAddress == null ? null : ipAddress!,
        'succeeded': succeeded,
      };
}

class AuditLogEntryPagedResultDto {
  const AuditLogEntryPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<AuditLogEntryDto> items;

  factory AuditLogEntryPagedResultDto.fromJson(Map<String, dynamic> json) => AuditLogEntryPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => AuditLogEntryDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

enum PasswordResetDelivery {
  email('email'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const PasswordResetDelivery(this.value);
  final String value;

  static PasswordResetDelivery fromJson(Object? json) {
    for (final v in PasswordResetDelivery.values) {
      if (v.value == json) return v;
    }
    return PasswordResetDelivery.$unknown;
  }

  String toJson() => value;
}

class PasswordResetResultDto {
  const PasswordResetResultDto({
    required this.userId,
    required this.deliveredVia,
    required this.expiresAt,
  });

  final String userId;
  final PasswordResetDelivery deliveredVia;
  final DateTime expiresAt;

  factory PasswordResetResultDto.fromJson(Map<String, dynamic> json) => PasswordResetResultDto(
        userId: (json['userId']) as String,
        deliveredVia: PasswordResetDelivery.fromJson(json['deliveredVia']),
        expiresAt: DateTime.parse((json['expiresAt']) as String),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'userId': userId,
        'deliveredVia': deliveredVia.toJson(),
        'expiresAt': hrDateTimeToJson(expiresAt),
      };
}

/// يُستخدم فقط لأهلية أنواع الإجازات. لا يظهر للمدير أو في EmployeeRefDto.
enum AppGender {
  male('male'),
  female('female'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const AppGender(this.value);
  final String value;

  static AppGender fromJson(Object? json) {
    for (final v in AppGender.values) {
      if (v.value == json) return v;
    }
    return AppGender.$unknown;
  }

  String toJson() => value;
}

class UserRefPagedResultDto {
  const UserRefPagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<UserRefDto> items;

  factory UserRefPagedResultDto.fromJson(Map<String, dynamic> json) => UserRefPagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => UserRefDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

class EvaluationCyclePagedResultDto {
  const EvaluationCyclePagedResultDto({
    required this.totalCount,
    required this.items,
  });

  final int totalCount;
  final List<EvaluationCycleDto> items;

  factory EvaluationCyclePagedResultDto.fromJson(Map<String, dynamic> json) => EvaluationCyclePagedResultDto(
        totalCount: ((json['totalCount']) as num).toInt(),
        items: ((json['items']) as List).map((e) => EvaluationCycleDto.fromJson((e) as Map<String, dynamic>)).toList(),
      );

  Map<String, dynamic> toJson() => <String, dynamic>{
        'totalCount': totalCount,
        'items': items.map((e) => e.toJson()).toList(),
      };
}

enum IncidentStatusTarget {
  inProgress('inProgress'),
  resolved('resolved'),

  /// A value this client does not know yet (newer server).
  $unknown('');

  const IncidentStatusTarget(this.value);
  final String value;

  static IncidentStatusTarget fromJson(Object? json) {
    for (final v in IncidentStatusTarget.values) {
      if (v.value == json) return v;
    }
    return IncidentStatusTarget.$unknown;
  }

  String toJson() => value;
}

