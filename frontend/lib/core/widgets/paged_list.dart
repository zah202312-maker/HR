import 'package:flutter/material.dart';

import '../l10n/strings.dart';
import 'common.dart';

typedef PageFetcher<T> = Future<(List<T>, int)> Function(int skipCount, int maxResultCount);

/// Give it a new Key to reload when filters change.
/// Contract list paging: skipCount/maxResultCount (default 20), totalCount + items.
class PagedList<T> extends StatefulWidget {
  const PagedList({
    super.key,
    required this.fetch,
    required this.itemBuilder,
    this.pageSize = 20,
    this.emptyMessage,
    this.header,
    this.padding = const EdgeInsets.only(bottom: 24),
  });

  final PageFetcher<T> fetch;
  final Widget Function(BuildContext context, T item) itemBuilder;
  final int pageSize;
  final String? emptyMessage;
  final Widget? header;
  final EdgeInsetsGeometry padding;

  @override
  State<PagedList<T>> createState() => PagedListState<T>();
}

class PagedListState<T> extends State<PagedList<T>> {
  final List<T> _items = <T>[];
  int _total = 0;
  bool _loading = false;
  Object? _error;

  @override
  void initState() {
    super.initState();
    reload();
  }

  Future<void> reload() async {
    setState(() {
      _items.clear();
      _total = 0;
      _error = null;
    });
    await _loadMore();
  }

  Future<void> _loadMore() async {
    if (_loading) return;
    setState(() => _loading = true);
    try {
      final (items, total) = await widget.fetch(_items.length, widget.pageSize);
      if (!mounted) return;
      setState(() {
        _items.addAll(items);
        _total = total;
      });
    } catch (e) {
      if (mounted) setState(() => _error = e);
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_error != null && _items.isEmpty) return ErrorState(error: _error!, onRetry: reload);
    if (_loading && _items.isEmpty) return const Center(child: CircularProgressIndicator());
    final hasMore = _items.length < _total;
    return RefreshIndicator(
      onRefresh: reload,
      child: ListView.separated(
        padding: widget.padding,
        physics: const AlwaysScrollableScrollPhysics(),
        itemCount: (widget.header != null ? 1 : 0) + (_items.isEmpty ? 1 : _items.length) + (hasMore ? 1 : 0),
        separatorBuilder: (_, __) => const SizedBox(height: 10),
        itemBuilder: (context, index) {
          var i = index;
          if (widget.header != null) {
            if (i == 0) return widget.header!;
            i--;
          }
          if (_items.isEmpty) return EmptyState(message: widget.emptyMessage);
          if (i < _items.length) return widget.itemBuilder(context, _items[i]);
          return Center(
            child: _loading
                ? const Padding(padding: EdgeInsets.all(12), child: CircularProgressIndicator())
                : TextButton(onPressed: _loadMore, child: Text(S.of(context).loadMore)),
          );
        },
      ),
    );
  }
}
